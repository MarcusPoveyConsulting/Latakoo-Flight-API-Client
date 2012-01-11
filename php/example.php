#!/usr/bin/php
<?php
	require_once('latakoo.php');
	
	$email = 'my email address';
	$password = 'my password';
	
	
	// Create API object using default endpoint
	$latakoo = new Latakoo();
	
	// List available APIs
	print_r($latakoo->systemApiList());
	

	// Authenticate a user
	try {
		if ($details = $latakoo->userAuthenticate($email, $password))
		{
			echo "User has been authenticated, here are their details...\n";
			print_r($details);
		}
	} catch (LatakooFlightException $e) {
		echo $e->getMessage();
	}


	// Get my details
	try {
		if ($me = $latakoo->userGetById($latakoo->getAuthenticatedUserID()))
		{
			echo "That's you that is...\n";
			print_r($me);
		}
	} catch (LatakooFlightException $e) {
		echo $e->getMessage();
	}
