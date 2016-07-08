#ifndef SOCIAL_MOVIES_H
#define SOCIAL_MOVIES_H

#include <string>
#include <vector>
#include <map>

// Enumeration of possible return values
enum ResultCode {

	// The function was successful 
	SUCCESS = 0,

	// An error condition was encountered when executing the function
	FAILURE,
};

class Movie
{
private:
	int _movieId;
	std::string _title;
	std::vector<Movie> _vSimilarMovies;
public:
	Movie() {}
	Movie(int movieId, const std::string& title)
	{
		_movieId = movieId;
		_title = title;
	}
	std::string GetTitle()
	{
		return _title;
	}
	void Associate(Movie movie)
	{
		_vSimilarMovies.push_back(movie);
	}
};

class Customer
{
private:
	int _customerId;
	std::string _name;
	std::vector<Customer> _vFriends;
public:
	Customer() {}
	Customer(int customerId, const std::string& name)
	{
		_customerId = customerId;
		_name = name;
	}
	std::string GetName()
	{
		return _name;
	}
};


// Maintains a network of movies and customers
// All methods should return ResultCode::SUCCESS when the operation completes successfully or
// ResultCode::FAILURE when an error condition occurs.
class SocialMovies
{
private:
	std::map<int, Movie> mMovies;
	std::map<int, Customer> mCustomers;
public:

	// Defines a movie ID to title mapping in the system
	ResultCode addMovie(int movieId, const std::string& title);

	// Gets the title of the given movie
	ResultCode lookupMovie(int movieId, std::string& movieTitle);

	// Defines a customer ID to name mapping in the system
	ResultCode addCustomer(int customerId, const std::string& name);

	// Gets the name of the given customer
	ResultCode lookupCustomer(int customerId, std::string& customerName);

	// Associate two movies as being similar
	ResultCode addSimilarMovie(int movieId1, int movieId2);

	// Associate two customers as being friends
	ResultCode addFriend(int customerId1, int customerId2);

	// Gets the IDs of all movies that are similar to the given movie
	ResultCode getSimilarMovies(int movieId, std::vector<int>& similarMovieIds);

	// Gets the smallest number of friend links required to traverse
	// the network between the two customers.
	// Example: If A is friends with B who is friends with C, getFriendDistance(A,C) := 2
	ResultCode getFriendDistance(int customerId1, int customerId2, int& distance);
};

#endif//SOCIAL_MOVIES_H
