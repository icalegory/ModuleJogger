/*
 * ManualTallyData.java
 *
 * $Revision: 126774 $ ($Date: 2015-11-26 19:59:36 -0500 (jue, 26 nov 2015) $)
 *
 * Copyright (c) 2004 Smartmatic Intl.
 * 1001 Broken Sound Parkway NW, Suite D
 * Boca Raton FL 33487, U.S.A.
 * All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Smartmatic Intl. ("Confidential Information"). You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered
 * into with Smartmatic Intl.
 */
package saes.election.manual;

import java.io.Serializable;
import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.Date;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Set;
import java.util.TreeSet;

import saes.common.BComponentEntity;
import saes.election.contest.ContestData;
import saes.election.demographics.LogicalVotingGroupData;
import saes.election.eligibleoption.PartyData;
import saes.election.manual.servlet.ManualTallyValidationUtil;
import saes.election.station.OptionRegisterData;
import saes.election.station.OptionRegisterPK;
import saes.election.station.ResponsibleStationTallyData;
import saes.election.station.StationTallyData;
import saes.exception.BaseRuntimeException;
import saes.gui.util.PageData;
import saes.metadata.BComponentAnnotation;
import saes.metadata.BComponentFieldAnnotation;

/**
 * <p>
 * JavaBean representing a manual station tally.
 * </p>
 * <p>
 * Manual tallies are those station tallies used in non-automated voting
 * processes. This kind of TALLY is used in those electoral events in which (for
 * whatever the reason) not all voting is done through an automated voting
 * machine, but some is done manually.
 * </p>
 * <p>
 * A manual TALLY is a consequence of one of the followings:
 * <ul>
 * <li>For any reason, a polling place in particular is unable to use voting
 * machines for voting (i.e. doesn't coutn with a proper electrical framework),
 * therefore all processes are done manually.</li>
 * <li>For any reason, a polling place that originally was meant to use
 * automated machines for voting no longer can and there is no contingency
 * availble. In these cases, all voting processes are switch to manual.</li>
 * <li>For any reason, a polling place performs all voting processes through
 * automated voting machines but data transmission through the network is not
 * possible and no other automated contingency is available. In these cases,
 * TALLY data transmission may be deviated to the manual framework as
 * contingency.</li>
 * </ul>
 * </p>
 * 
 * @author Fernando Hernandez fhernandez@smartmatic.com
 * @version 1.0 (Feb 11, 2005)
 * @hibernate.class table="manual_tally" optimistic-lock="version"
 *                  dynamic-update="true" select-before-update="true"
 */
@BComponentAnnotation(pathToAuthority = "contest.electoralAuthority")
public class ManualTallyData extends BComponentEntity implements Serializable {

    /**
     * <p>
     * Inner class that implements the {@link Comparator} interface and is used
     * to compare the ManualTally objects.
     * </p>
     * <p>
     * Copyright: Copyright (c) 2004
     * </p>
     * <p>
     * Company:
     * </p>
     * 
     * @author not attributable
     * @version 1.0
     */
    private class OrdenComparator implements Comparator {

        /* Creates a new empty instance of the OrdenComparator Class. */
        /**
         * Instantiates a new orden comparator.
         */
        public OrdenComparator() {
            super();
        }

        /**
         * implementation of the compare method, in this implementation is used
         * the manualArrange property of the ballotOption.
         * 
         * @param o1
         *            Object
         * @param o2
         *            Object
         * @return int
         */
        @Override
        public int compare(Object o1, Object o2) {

            ManualOptionRegisterData r1 = (ManualOptionRegisterData) o1;
            ManualOptionRegisterData r2 = (ManualOptionRegisterData) o2;

            // Always show the VTV as the last option per each original eligible
            // option group.
            if (r1.getBallotOption().getOriginalEligible()
                    .equals(r2.getBallotOption().getOriginalEligible())) {
                boolean r1CFO = r1.getBallotOption().getParty().getType()
                        .equals(PartyData.Type.CFO);
                boolean r2CFO = r2.getBallotOption().getParty().getType()
                        .equals(PartyData.Type.CFO);
                if (r1CFO ^ r2CFO) {
                    return r1CFO ? 1 : -1;
                }
            }

            int orden1 = r1.getBallotOption().getManualArrange();
            int orden2 = r2.getBallotOption().getManualArrange();

            return orden1 - orden2;
        }

        /**
         * Implementation of the equals method.
         * 
         * @param obj
         *            Object
         * @return boolean
         */
        @Override
        public boolean equals(Object obj) {
            return obj instanceof OrdenComparator;
        }
    }

    /**
     * The possibles status of the ManualTally.
     * 
     * @author Smartmatic Intl.
     * @version 1.0
     */
    public enum Status {
        /*
         * <p> The manual TALLY is already registered inside the REIS system as
         * a manual TALLY, but hasn't been counted as a regular station TALLY
         * for final tabulation. </p> <p> This status counts for all possible
         * manual tallies scheme configured in REIS (e.g.
         * double-insertion-validation-certification,
         * single-insertion-validation-no-certification,
         * insertion-certification, just-insertion, etc.) </p>
         */
        /** The inserted. */
        INSERTED,

        /*
         * <p> The manual TALLY is already registered inside the REIS system as
         * a manual TALLY and somehow there has been verifications over the
         * quality of the data in it (how the verification was made is no
         * relevant at this point). Additionally, this manual TALLY hasn't been
         * counted as a regular station TALLY for final tabulation. </p> <p>
         * This status counts only for those manual tallies schemes configured
         * to require any kind of TALLY-data quality validation (e.g.
         * double-insertion-validation-certification.). </p>
         */
        /** The verified. */
        VERIFIED,

        /*
         * <p> The manual TALLY is already registered inside the REIS system as
         * a manual TALLY, but somehow data quality assurance has failed to
         * approve it as a valid manual TALLY (how the verification was made is
         * no relevant at this point). As a consequence, there is no possible
         * way that this manual TALLY is counted as a regular station TALLY for
         * final tabulation. <p> This status counts only for those manual
         * tallies schemes configured to require any kind of TALLY-data quality
         * validation (e.g. double-insertion-validation-certification. </p>
         */
        /** The reinsert. */
        REINSERT,

        /*
         * <p> The manual TALLY is already registered inside the REIS system as
         * a manual TALLY and it has been approved and counted as a regular
         * station TALLY for final tabulation. </p> <p> Whether this
         * certification required any kind of data-quality validation is not
         * reflected in this status (this should be reflected in this manual
         * TALLY corresponding history). Only the fact that the TALLY is taken
         * into account for final tabulation. </p>
         */
        /** The certified. */
        CERTIFIED,

        /*
         * In the very unlikely and inprobable case in which the same TALLY
         * arrives at the system through automated process before or after the
         * same corresponding manual TALLY has been inserted in REIS (it is
         * unlikely because no TALLY is expected through both mediums, but still
         * it's not impossible), the system gives precedence to the automated
         * TALLY as a general rule, so if a case like this occurs, the system
         * marks the manual TALLY status as "AUTOMATED_PREVAILED", discards it
         * for final tabulation purposes and works with the automated station
         * TALLY received thorugh the secured network.
         */
        /** The automated prevailed. */
        AUTOMATED_PREVAILED,

        /* The manual tally page is complete and manual tally is ready for Certificate.*/
        /** The ready. */
        READY,

        /* The manual tally does not meet the requirements to be certified  */
        /** The denied. */
        DENIED,

        /* The manual tally contains observations */
        /** The observation. */
        OBSERVATION,
    }

    /** the prefix used in the log. */
    public static final String LOG_PREFIX = ManualTallyData.class
            .getCanonicalName() + ".";

    /**
     * <p>
     * Distinctive code for this ManualTally. This code is only used by the
     * system and has no semantic value for any of application's users.
     * </p>
     * <p>
     * The values for this field must be auto generated by the database manager.
     * </p>
     */
    private Long code;

    /**
     * <p>
     * the CustomCode.
     * </p>
     */
    private String customCode;

    /**
     * <p>
     * Status of the ManualTally
     * </p>
     * .
     */
    private Status status;

    /**
     * <p>
     * the Contest related to the manualTally
     * </p>
     * .
     */
    private ContestData contest;

    /**
     * <p>
     * date and time where was received the manualTally
     * </p>
     * .
     */
    private Date receptionDate;

    /** date and time of issue of the manualTally. */
    private String issuingDate;

    /** Represents the logical voting group that the manualTally belongs. */
    private LogicalVotingGroupData logicalVotingGroup;

    /**
     * <p>
     * the voters amount
     * </p>
     * .
     */
    private BigDecimal voters;

    /**
     * <p>
     * amount of ballots in the Box
     * </p>
     * .
     */
    private BigDecimal processedBallots;

    /**
     * <p>
     * valid votes amount.
     * </p>
     */
    private BigDecimal validVotes;

    /**
     * <p>
     * total votes amount.
     * </p>
     */
    private BigDecimal totalVotes;


    /** The nota contests total. */
    private BigDecimal notaContestsTotal;

    /**
     * <p>
     * observation about possibles wrongs about the manualTally.
     * </p>
     */
    private String observations;

    /**
     * <p>
     * the system observation about the manualTally.
     * </p>
     */
    private String systemObservations;

    /**
     * <p>
     * the collection of responsible of the manualTally.
     * </p>
     */
    private List<ManualTallyResponsibleData> responsibles = new ArrayList<ManualTallyResponsibleData>();

    /**
     * <p>
     * collection off all the result of manualTally.
     * </p>
     */
    private Set<ManualOptionRegisterData> manualOptionRegisters;

    /**
     * <p>
     * version of the manualTally.
     * </p>
     */
    private int version;

    /**
     * Amount of editions on the manual tally.
     */
    private int editions;

    /**
     * Amount of insertions on the manual tally.
     */
    private int insertions;

    /**
     * <p>
     * the last responsible of the system.
     * </p>
     */
    private String systemLastResponsible;

    /**
     * <p>
     * the stationTally.
     * </p>
     */
    private StationTallyData stationTally;

    /**
     * <p>
     * the collection of pages of manual tally.
     * </p>
     */
    private Set<ManualTallyPageImageData> manualTallyPageImages = new TreeSet<ManualTallyPageImageData>();

   
    /** The blank contests total. */
    private BigDecimal blankContestsTotal;

    /**
     * Creates a new empty instance of the ManualTallyData Class.
     */

    public ManualTallyData() {
        this.version = 0;
        this.insertions = 0;
    }

    /**
     * Overriding of the equals method, in this implementation are used the
     * {@link #processedBallots}, {@link #notaContestsTotal}, {@link #blankContestsTotal},
     * {@link #issuingDate}, {@link #customCode}, {@link #voters},
     * {@link #observations}, {@link #status} and {@link #logicalVotingGroup} to
     * perform the equals
     * comparison.
     * 
     * @param o
     *            the Object to be compared.
     * @return true if the objects are equals, false if not.
     */
    @Override
    public boolean equals(Object o) {
        ManualTallyData acta = (ManualTallyData) o;
        boolean result = false;
        try {
            boolean processedBallotsResult = acta.processedBallots.equals(this.processedBallots);
            boolean notaContestsTotalResult = acta.notaContestsTotal.equals(this.notaContestsTotal);
            boolean blankContestsTotalResult = acta.blankContestsTotal.equals(this.blankContestsTotal);
            boolean contestResult = acta.contest.equals(this.contest);
            boolean issueDateResult = (acta.issuingDate != null)
                    && (this.issuingDate != null) || (acta.issuingDate == null)
                    && (this.issuingDate == null);
            boolean tallyNumberResult = acta.customCode.equals(this.customCode);
            boolean votersResult = acta.voters.equals(this.voters);
            boolean observationsResult = (acta.observations != null)
                    && (this.observations != null)
                    || (acta.observations == null)
                    && (this.observations == null);
            boolean tallyStatusResult = acta.status.equals(this.status);

            boolean lvgResult = acta.logicalVotingGroup
                    .equals(this.logicalVotingGroup);

            result = processedBallotsResult && blankContestsTotalResult && notaContestsTotalResult
                    && contestResult
                    && issueDateResult && tallyNumberResult
                    && votersResult && observationsResult && tallyStatusResult
                    && this.voteEquals(acta) && lvgResult;

        } catch (NullPointerException npe) {
            result = false;
        }
        return result;
    }

    /**
     * <p>
     * Getter for the <code>processedBallots</code> attribute.
     * </p>
     * 
     * @return {@link #processedBallots}
     * @hibernate.property type="big_decimal"
     * @hibernate.column name="processed_ballots"
     */
    public BigDecimal getProcessedBallots() {
        return this.processedBallots;
    }

    /**
     * <p>
     * Getter for the <code>code</code> attribute.
     * 
     * @return Integer
     * @hibernate.id column="manual_tally_code" generator-class="native"
     *               unsaved-value="0" length="12"
     * @hibernate.generator-param name="sequence" value="S_MANUAL_TALLY"
     */
    @BComponentFieldAnnotation(flagPK = true)
    public Long getCode() {

        return this.code;
    }

    /**
     * <p>
     * Getter for the <code>contest</code> attribute.
     * </p>
     * 
     * @return the {@link #contest}
     * @hibernate.many-to-one
     * @hibernate.column name="contest_code"
     */
    public ContestData getContest() {
        return this.contest;
    }

    /**
     * <p>
     * Getter for the <code>customCode</code> attribute.
     * 
     * @return the {@link #customCode} property.
     * @hibernate.property column = "manual_tally_custom_code" length = "80"
     *                     not-null = "true" unique = "true"
     */
    @BComponentFieldAnnotation(flagName = true, flagMandatory = true)
    public String getCustomCode() {
        return this.customCode;
    }

    /**
     * Getter for the <code>editions</code> attribute.
     * 
     * @return the {@link #editions} property.
     * @hibernate.property column="editions"
     */
    public int getEditions() {
        return this.editions;
    }

    /**
     * <p>
     * Getter for the <code>blankContestsTotal</code> attribute.
     * </p>
     * 
     * @return the {@link #blankContestsTotal} property.
     * @hibernate.property type="big_decimal"
     * @hibernate.column name="blank_contests_total"
     */
    public BigDecimal getBlankContestsTotal() {
        return this.blankContestsTotal;
    }

    /**
     * Getter for the <code>insertions</code> attribute.
     * 
     * @return the {@link #insertions} property.
     * @hibernate.property column="insertions"
     */
    public int getInsertions() {
        return this.insertions;
    }

    /**
     * Gets a String IpInsercion from the systemLastResponsible.
     * 
     * @return String
     */
    public String getIpInsercion() {
        return this.systemLastResponsible.substring(this.systemLastResponsible
                .indexOf(ManualTallyValidationUtil.AT) + 1);
    }

    /**
     * Getter for the <code>issuingDate</code> attribute.
     * 
     * @return the {@link #issuingDate} property.
     * @hibernate.property column="date_of_issue"
     */
    public String getIssuingDate() {
        return this.issuingDate;
    }

    /**
     * Gets the sysmtem last responsible.
     * 
     * @return String
     */
    public String getLastResponsible() {
        return this.systemLastResponsible.substring(0,
                this.systemLastResponsible
                        .indexOf(ManualTallyValidationUtil.AT));
    }

    /**
     * Getter of the logical voting booth property.
     * 
     * @return the logicalVotingGroup
     * @hibernate.many-to-one column="logical_vg_code" not-null="true"
     *                        lazy="false"
     */
    public LogicalVotingGroupData getLogicalVotingGroup() {
        return this.logicalVotingGroup;
    }

    /**
     * <p>
     * Descripcion: Getter de la propiedad manualOptionRegisters
     * </p>
     * .
     * 
     * @return Set Valor de la propiedad
     * @hibernate.set sort="natural" cascade="all" inverse="true" lazy="true"
     * @hibernate.collection-key column="manual_tally_code"
     * @hibernate.collection-one-to-many class=
     *                                   "saes.election.manual.ManualOptionRegisterData"
     *                                   insert="true" update="true"
     */
    public Set<ManualOptionRegisterData> getManualOptionRegisters() {
        if (this.manualOptionRegisters == null) {
            this.manualOptionRegisters = new HashSet<ManualOptionRegisterData>();
        }
        return this.manualOptionRegisters;
    }
    
    /**
     * Gets the manual opt reg ordered.
     * 
     * @return List
     */
    public List<ManualOptionRegisterData> getManualOptRegOrdered() {
        List<ManualOptionRegisterData> escrutiniosActa = new ArrayList(
                this.manualOptionRegisters);
        Collections.sort(escrutiniosActa, new OrdenComparator());
        return escrutiniosActa;
    }

    /**
     * <p>
     * Getter ManualTallyPageImages
     * </p>
     * .
     * 
     * @return Set ManualTallyPageImageData
     * @hibernate.set sort="natural" cascade="all" inverse="true" lazy="true"
     * @hibernate.collection-key column="MANUAL_TALLY_CODE"
     * @hibernate.collection-one-to-many class=
     *                                   "saes.election.manual.ManualTallyPageImageData"
     */
    public Set<ManualTallyPageImageData> getManualTallyPageImages() {
        return this.manualTallyPageImages;
    }

    /**
     * <p>
     * Getter for the <code>notaContestsTotal</code> attribute.
     * </p>
     * 
     * @return the {@link #notaContestsTotal} property.
     * @hibernate.property type="big_decimal"
     * @hibernate.column name="nota_contests_total"
     */
    public BigDecimal getNotaContestsTotal() {
        return this.notaContestsTotal;
    }

    /**
     * Creates a collection of logs or Exceptions that indicates the
     * observations in the ManualTally.
     * 
     * @param data
     *            the data
     * @return List
     */
    public List getObservacionesCertificador(PageData data) {

        List resultado = new ArrayList();
        BigDecimal eligibleAmount = new BigDecimal(this.contest
                .getNumberOfPositions());
        BigDecimal totalCantidadVotos = new BigDecimal(0);

        /* Iterates over the manualOptionRegister */
        for (Iterator i = this.manualOptionRegisters.iterator(); i.hasNext();) {
            ManualOptionRegisterData optionRegister = (ManualOptionRegisterData) i
                    .next();
            int codigoOpcionBoleta = optionRegister.getBallotOption().getCode();

            /* Checks the amount of obtaind votes */
            if (optionRegister.getAmount().intValue() > this
                    .getLogicalVotingGroup().getVotingBooth().getAmountVoters()) {
                resultado.add(new Exception(data.getText(
                        "manualtally.exceed", codigoOpcionBoleta)));
            } else {
                totalCantidadVotos = totalCantidadVotos.add(optionRegister
                        .getAmount());
            }
        }

        /*
         * Chech the amount of electors with the quantity of ballots inserted in
         * the box.
         */
        if (this.voters.compareTo(this.processedBallots) > 0) {
            resultado
                    .add(ManualTallyData.LOG_PREFIX
                            + "MANUAL_TALLY.VOTERS_REGISTERED_IN_BOOK.MORE_THAN.BALLOTS_ACCOUNTED_IN_BOX");
        }
        if (this.voters.compareTo(this.processedBallots) < 0) {
            resultado
                    .add(ManualTallyData.LOG_PREFIX
                            + "MANUAL_TALLY.VOTERS_REGISTERED_IN_BOOK.LESS_THAN.BALLOTS_ACCOUNTED_IN_BOX");
        }

        BigDecimal totalVotos = this.getTotalVotos();

        /*
         * Check the quantity of ballots inserted in the box with the total
         * amount of votes.
         */
        boolean oneOptionContest = eligibleAmount.intValue() == 1;
        if (oneOptionContest) {
            if (this.processedBallots.compareTo(totalVotos) > 0) {
                resultado
                        .add(ManualTallyData.LOG_PREFIX
                                + "MANUAL_TALLY.BALLOTS_ACCOUNTED_IN_BOX.MORE_THAN.TOTAL_VOTES");
            }
        }

        if (this.processedBallots.multiply(eligibleAmount).compareTo(totalVotos) < 0) {
            String message = "MANUAL_TALLY.BALLOTS_ACCOUNTED_IN_BOX.LESS_THAN.TOTAL_VOTES";
            if (!oneOptionContest) {
                message = "MANUAL_TALLY.BALLOTS_ACCOUNTED_IN_BOX_BY_ELIGIBLE_AMOUNT.LESS_THAN.TOTAL_VOTES";
            }
            resultado.add(ManualTallyData.LOG_PREFIX + message);
        }

        /*
         * Check the quantity of electors with the quantity of ballots inserted
         * in the box.
         */
        if (oneOptionContest) {
            if (this.voters.compareTo(totalVotos) > 0) {
                resultado.add(ManualTallyData.LOG_PREFIX
                        + "MANUAL_TALLY.VOTERS.MORE_THAN.TOTAL_VOTES");
            }
        }
        if (this.voters.multiply(eligibleAmount).compareTo(totalVotos) < 0) {
            String message = "MANUAL_TALLY.VOTERS.LESS_THAN.TOTAL_VOTES";
            if (!oneOptionContest) {
                message = "MANUAL_TALLY.VOTERS_BY_ELIGIBLE_AMOUNT.LESS_THAN.TOTAL_VOTES";
            }
            resultado.add(ManualTallyData.LOG_PREFIX + message);
        }

        if (this.voters.compareTo(new BigDecimal(this.getLogicalVotingGroup()
                .getVotingBooth().getAmountVoters())) > 0) {
            resultado
                    .add(new BaseRuntimeException(
                            ManualTallyData.LOG_PREFIX
                                    + "MANUAL_TALLY.VOTERS_REGISTERED_IN_BOOK.EXCEED.VOTERS_REGISTRY"));// OK
        }
        /*
         * Validates that there is no more voters registered that the quantity
         * registered in the votingBooth.
         */
        if ((totalVotos != null)
                && (this.getLogicalVotingGroup().getVotingBooth()
                        .getAmountVoters()
                        * this.contest.getNumberOfPositions() < totalVotos
                            .intValue())) {
            resultado.add(new BaseRuntimeException(ManualTallyData.LOG_PREFIX
                    + "MANUAL_TALLY.VOTES.PDF.TOTAL.EXCEEDS_REGISTRY"));// OK
        }

        return resultado;
    }

    /**
     * <p>
     * Getter for the <code>observations</code> attribute.
     * </p>
     * 
     * @return the {@link #observations} property.
     * @hibernate.property column="observation" length="2000"
     */
    public String getObservations() {
        return this.observations;
    }

    /**
     * <p>
     * Getter for the <code>receptionDate</code> attribute.
     * </p>
     * 
     * @return {@link #receptionDate}
     * @hibernate.property column="reception_date" not-null="true"
     */
    public Date getReceptionDate() {
        return this.receptionDate;
    }

    /**
     * Getter for the <code>responsibles</code> attribute.
     * 
     * @return the {@link #responsibles} property.
     * @hibernate.list table="manual_tally_resp" inverse="false" sort="natural"
     *                 cascade="all" lazy="true"
     * @hibernate.collection-key column="manual_tally_code"
     * @hibernate.collection-index column = "man_tally_resp_code"
     * @hibernate.collection-composite-element
     *                                         class=
     *                                         "saes.election.manual.ManualTallyResponsibleData"
     */
    public List<ManualTallyResponsibleData> getResponsibles() {
        return this.responsibles;
    }

    /**
     * <p>
     * Builds a stationTally with the necessary values and return it.
     * </p>
     * 
     * @return StationTallyData
     */
    public StationTallyData getStationTallyData() {

        /* Create the new empty StationTally */
        this.stationTally = new StationTallyData();

        /* Sets the values */
        this.stationTally.setProcessedBallots(this.processedBallots);
        this.stationTally.setNotaContestsTotal(this.notaContestsTotal);
        this.stationTally.setBlankContestsTotal(this.blankContestsTotal);
        this.stationTally.setContest(this.contest);
        this.stationTally.setCode(this.code);
        this.stationTally.setSystemReceptionDate(this.receptionDate);
        this.stationTally.setNumber(this.customCode);
        this.stationTally.setVotersAmount(this.voters);

        if (this.observations != null && this.systemObservations != null) {
            this.stationTally.setComments(this.observations + " - "
                    + this.systemObservations);
        } else if (this.observations != null) {
            this.stationTally.setComments(this.observations);
        } else if (this.systemObservations != null) {
            this.stationTally.setComments(this.systemObservations);
        }

        this.stationTally.setStatus(0);
        this.stationTally.setType(StationTallyData.MANUAL);
        this.stationTally.setLogicalVotingGroup(this.logicalVotingGroup);
        this.stationTally.setTallied(false);
        Set resultadosAsAED = new HashSet();

        /* Iterates over the manualOptionRegisters collection */
        for (Iterator i = this.manualOptionRegisters.iterator(); i.hasNext();) {
            ManualOptionRegisterData resultadoInput = (ManualOptionRegisterData) i
                    .next();

            /* Creates a new OptionRegister per each iteration */
            OptionRegisterData resultadoOutput = new OptionRegisterData();
            OptionRegisterPK resultadoOutputPK = new OptionRegisterPK();

            /* Sets the values */
            resultadoOutputPK.setStationTally(this.stationTally);
            resultadoOutputPK.setBallotOption(resultadoInput.getBallotOption());

            resultadoOutput.setOptionRegisterPK(resultadoOutputPK);
            BigDecimal amount = resultadoInput.getAmount();
            resultadoOutput.setAmount(amount == null ? new BigDecimal(0)
                    : amount);

            /* Adds the element to the set */
            resultadosAsAED.add(resultadoOutput);
        }

        this.stationTally.setOptionRegisterList(resultadosAsAED);

        /* Creates the collection of Responsible */
        Set<ResponsibleStationTallyData> responsables = new LinkedHashSet<ResponsibleStationTallyData>();

        /* Iterates over the collection of responsible */
        for (Object theElement : this.responsibles) {

            /* CReates a new ManualTallyResponsible per each iteration */
            ManualTallyResponsibleData resIter = (ManualTallyResponsibleData) theElement;
            ResponsibleStationTallyData responsable = new ResponsibleStationTallyData();

            /* Sets the values */
            responsable.setId(resIter.getId());
            responsable.setRole(null);
            responsable.setCode(null);
            responsables.add(responsable);
        }

        /* Set the collection of responsible to the stationTally */
        this.stationTally.setResponsiblesList(responsables);

        return this.stationTally;
    }

    /**
     * <p>
     * Getter for the <code>status</code> attribute.
     * </p>
     * 
     * @return the {@link #status}
     */
    public Status getStatus() {
        return this.status;
    }

    /**
     * Getter for the key (in property file) related to each status.
     * 
     * @return String key
     */
    public String getStatusLabel() {
        return Status.class.getCanonicalName() + ".lang." + this.status;
    }

    /**
     * Getter for the <code>systemLastResponsible</code> attribute.
     * 
     * @return the {@link #systemLastResponsible} property.
     * @hibernate.property column="resp_last"
     */
    public String getSystemLastResponsible() {
        return this.systemLastResponsible;
    }

    /**
     * <p>
     * Getter for the <code>systemObservations</code> attribute.
     * </p>
     * 
     * @return the {@link #systemObservations} property.
     * @hibernate.property column="system_observations" length="2000"
     */
    public String getSystemObservations() {
        return this.systemObservations;
    }

    /**
     * <p>
     * Getter for the <code>totalVotes</code> attribute.
     * </p>
     * 
     * @return the {@link #totalVotes} property.
     * @hibernate.property type="big_decimal"
     * @hibernate.column name="total_amount"
     */
    public BigDecimal getTotalVotes() {
        return this.totalVotes;
    }

    /**
     * Obtains and return the total of votes.
     * 
     * @return the total of votes.
     */
    public BigDecimal getTotalVotos() {

        BigDecimal totalCantidadVotos = new BigDecimal(0);
        BigDecimal candidateAmount = null;

        /* Iterates over the manualOptionRegisters collection */
        for (Iterator i = this.manualOptionRegisters.iterator(); i.hasNext();) {
            ManualOptionRegisterData resultado = (ManualOptionRegisterData) i
                    .next();

            /* Increments the amount of votes */
            candidateAmount = resultado.getAmount();
            if (candidateAmount != null) {
                totalCantidadVotos = totalCantidadVotos.add(candidateAmount);
            }
        }

        if (this.notaContestsTotal != null) {
            totalCantidadVotos = totalCantidadVotos.add(this.notaContestsTotal);
        }

        if (this.blankContestsTotal != null) {
            totalCantidadVotos = totalCantidadVotos.add(this.blankContestsTotal);
        }
        return totalCantidadVotos;
    }

    /**
     * <p>
     * Getter for the <code>validVotes</code> attribute.
     * </p>
     * 
     * @return the {@link #validVotes} property.
     * @hibernate.property type="big_decimal"
     * @hibernate.column name="valid_amount"
     */
    public BigDecimal getValidVotes() {
        return this.validVotes;
    }

    /**
     * Getter for the <code>version</code> attribute.
     * 
     * @return then {@link #version} property.
     * @hibernate.version access = "property" column="version"
     *                    property="version"
     */
    public int getVersion() {
        return this.version;
    }

    /**
     * <p>
     * Getter for the <code>voters</code> attribute.
     * </p>
     * 
     * @return the {@link #voters} property
     * @hibernate.property type="big_decimal"
     * @hibernate.column name="voters_amount"
     */
    public BigDecimal getVoters() {
        return this.voters;
    }

    /**
     * Setter for the <code>processedBallots</code> attribute.
     * 
     * @param theProcessedBallots
     *            the <code>processedBallots</code> to set
     * @see #processedBallots
     */
    public void setProcessedBallots(BigDecimal theProcessedBallots) {
        this.processedBallots = theProcessedBallots;
    }

    /**
     * Setter for the code attribute.
     * 
     * @param code
     *            the code property.
     */
    public void setCode(Long code) {
        this.code = code;
    }

    /**
     * <p>
     * Setter of the <code>contest</code> property
     * </p>
     * .
     * 
     * @param contest
     *            the {@link #contest} property to be setted.
     */
    public void setContest(ContestData contest) {
        this.contest = contest;
    }

    /**
     * Setter of the <code>customCode</code> property.
     * 
     * @param customCode
     *            the {@link #customCode} property.
     */
    public void setCustomCode(String customCode) {
        this.customCode = customCode;
    }

    /**
     * Setter of the editions attribute.
     * 
     * @param ediciones
     *            the new amount of editions on the manual tally
     */
    public void setEditions(int ediciones) {
        this.editions = ediciones;
    }

    /**
     * Setter for the <code>blankContestsTotal</code> attribute.
     * 
     * @param theBlankContestsTotal
     *            the <code>blankContestsTotal</code> to set
     * @see #blankContestsTotal
     */
    public void setBlankContestsTotal(BigDecimal theBlankContestsTotal) {
        this.blankContestsTotal = theBlankContestsTotal;
    }

    /**
     * Setter of the insertions attribute.
     * 
     * @param inserciones
     *            the new amount of insertions on the manual tally
     */
    public void setInsertions(int inserciones) {
        this.insertions = inserciones;
    }

    /**
     * Setter of the <code>issuingDate</code> property.
     * 
     * @param fechaEmision
     *            the new date and time of issue of the manualTally
     */
    public void setIssuingDate(String fechaEmision) {
        this.issuingDate = fechaEmision;
    }

    /**
     * Setter for the <code>logicalVotingGroup</code> attribute.
     * 
     * @param theLogicalVotingGroup
     *            the <code>logicalVotingGroup</code> to set
     * @see #logicalVotingGroup
     */
    public void setLogicalVotingGroup(
            LogicalVotingGroupData theLogicalVotingGroup) {
        this.logicalVotingGroup = theLogicalVotingGroup;
    }

    /**
     * Setter for the <code>manualOptionRegisters</code> attribute.
     * 
     * @param theManualOptionRegisters
     *            the <code>manualOptionRegisters</code> to set
     * @see #manualOptionRegisters
     */
    public void setManualOptionRegisters(
            Set<ManualOptionRegisterData> theManualOptionRegisters) {
        this.manualOptionRegisters = theManualOptionRegisters;
    }

    /**
     * Setter for the <code>manualTallyPageImages</code> attribute.
     * 
     * @param theManualTallyPageImageData
     *            the new
     *            <p>
     *            the collection of pages of manual tally
     */
    public void setManualTallyPageImages(
            Set<ManualTallyPageImageData> theManualTallyPageImageData) {
        this.manualTallyPageImages = theManualTallyPageImageData;
    }

    /**
     * Setter for the <code>notaContestsTotal</code> attribute.
     * 
     * @param theNotaContestsTotal
     *            the <code>notaContestsTotal</code> to set
     * @see #notaContestsTotal
     */
    public void setNotaContestsTotal(BigDecimal theNotaContestsTotal) {
        this.notaContestsTotal = theNotaContestsTotal;
    }

    /**
     * Setter for the <code>observations</code> attribute.
     * 
     * @param theObservations
     *            the <code>observations</code> to set
     * @see #observations
     */
    public void setObservations(String theObservations) {
        this.observations = theObservations;
    }

    /**
     * <p>
     * Setter of the <code>receptionDate</code> property
     * </p>
     * .
     * 
     * @param fechaRecepcion
     *            the new
     *            <p>
     *            date and time where was received the manualTally
     *            </p>
     */
    public void setReceptionDate(Date fechaRecepcion) {
        this.receptionDate = fechaRecepcion;
    }

    /**
     * Setter of the responsibles attribute.
     * 
     * @param responsables
     *            the new
     *            <p>
     *            the collection of responsible of the manualTally
     */
    public void setResponsibles(List responsables) {
        this.responsibles = responsables;

    }

    /**
     * <p>
     * Setter of the <code>status</code> property
     * </p>
     * .
     * 
     * @param status
     *            the {@link #status} property to be setted.
     */
    public void setStatus(Status status) {
        this.status = status;
    }

    /**
     * Setter for the <code>systemLastResponsible</code> attribute.
     * 
     * @param theSystemLastResponsible
     *            the <code>systemLastResponsible</code> to set
     * @see #systemLastResponsible
     */
    public void setSystemLastResponsible(String theSystemLastResponsible) {
        this.systemLastResponsible = theSystemLastResponsible;
    }

    /**
     * Setter of the systemObservations attribute.
     * 
     * @param theSystemObservations
     *            the systemObservations to set
     */
    public void setSystemObservations(String theSystemObservations) {
        this.systemObservations = theSystemObservations;
    }

    /**
     * Setter for the <code>totalVotes</code> attribute.
     * 
     * @param theTotalVotes
     *            the <code>totalVotes</code> to set
     * @see #totalVotes
     */
    public void setTotalVotes(BigDecimal theTotalVotes) {
        this.totalVotes = theTotalVotes;
    }

    /**
     * Setter for the <code>validVotes</code> attribute.
     * 
     * @param theValidVotes
     *            the <code>validVotes</code> to set
     * @see #validVotes
     */
    public void setValidVotes(BigDecimal theValidVotes) {
        this.validVotes = theValidVotes;
    }

    /**
     * Setter for the <code>version</code> attribute.
     * 
     * @param theVersion
     *            the <code>version</code> to set
     * @see #version
     */
    public void setVersion(int theVersion) {
        this.version = theVersion;
    }

    /**
     * <p>
     * Setter of the <code>voters</code> property
     * </p>
     * .
     * 
     * @param numeroElectores
     *            the new
     *            <p>
     *            the voters amount
     *            </p>
     */
    public void setVoters(BigDecimal numeroElectores) {
        this.voters = numeroElectores;
    }

    /**
     * Checks if a manualOption register of the received manualTally is equals
     * to the manualOptionRegisters of this ManualTally.
     * 
     * @param manualTally
     *            The tally to compare the votes
     * 
     * @return boolean true if both manualOptionRegister are equals, false if
     *         not.
     */

    private boolean voteEquals(ManualTallyData manualTally) {
        boolean result = true;

        Iterator resultadoIterObject = manualTally.manualOptionRegisters
                .iterator();

        while (resultadoIterObject.hasNext() && result) {
            ManualOptionRegisterData resultadoObject = (ManualOptionRegisterData) resultadoIterObject
                    .next();
            boolean resultFound = false;
            Iterator thisResultadoIter = this.manualOptionRegisters.iterator();
            while (thisResultadoIter.hasNext() && result && !resultFound) {
                ManualOptionRegisterData thisResultado = (ManualOptionRegisterData) thisResultadoIter
                        .next();
                if (thisResultado.equals(resultadoObject)) {
                    resultFound = true;
                    if (!thisResultado.getAmount().equals(
                            resultadoObject.getAmount())) {
                        result = false;
                        return result;
                    }
                }
                if (!thisResultadoIter.hasNext() && !resultFound) {
                    result = false;
                    return result;
                }
            }
        }
        return result;
    }

}
