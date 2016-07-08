#include <string>
#include <vector>
#include <map>
#include "SocialMovies.h"

// Maintains a network of movies and customers
// All methods should return ResultCode::SUCCESS when the operation completes successfully or
// ResultCode::FAILURE when an error condition occurs.

// Defines a movie ID to title mapping in the system
ResultCode SocialMovies::addMovie(int movieId, const std::string& title)
{
	mMovies[movieId] = Movie(movieId, title);
	return ResultCode::SUCCESS;
}

// Gets the title of the given movie
ResultCode SocialMovies::lookupMovie(int movieId, std::string& movieTitle)
{
	if (mMovies.count(movieId) > 0)
	{
		movieTitle = mMovies[movieId].GetTitle();
		return ResultCode::SUCCESS;
	}
	else
	{
		return ResultCode::FAILURE;
	}
}

// Defines a customer ID to name mapping in the system
ResultCode SocialMovies::addCustomer(int customerId, const std::string& name)
{
	mCustomers[customerId] = Customer(customerId, name);
	return ResultCode::SUCCESS;
}

// Gets the name of the given customer
ResultCode SocialMovies::lookupCustomer(int customerId, std::string& customerName)
{
	if (mCustomers.count(customerId) > 0)
	{
		customerName = mCustomers[customerId].GetName();
		return ResultCode::SUCCESS;
	}
	else
	{
		return ResultCode::FAILURE;
	}
}

// Associate two movies as being similar
ResultCode SocialMovies::addSimilarMovie(int movieId1, int movieId2)
{
	if (mMovies.count(movieId1) > 0 && mMovies.count(movieId2) > 0)
	{
		mMovies[movieId1].Associate(mMovies[movieId2]);
		return ResultCode::SUCCESS;
	}
	else
	{
		return ResultCode::FAILURE;
	}
}

// Associate two customers as being friends
ResultCode SocialMovies::addFriend(int customerId1, int customerId2)
{

	return ResultCode::SUCCESS;
}

// Gets the IDs of all movies that are similar to the given movie
ResultCode SocialMovies::getSimilarMovies(int movieId, std::vector<int>& similarMovieIds)
{

	return ResultCode::SUCCESS;
}

// Gets the smallest number of friend links required to traverse
// the network between the two customers.
// Example: If A is friends with B who is friends with C, getFriendDistance(A,C) := 2
ResultCode SocialMovies::getFriendDistance(int customerId1, int customerId2, int& distance)
{

	return ResultCode::SUCCESS;
}

