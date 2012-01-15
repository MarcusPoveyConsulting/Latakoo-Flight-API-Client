#!/usr/bin/ruby

require "latakoo"
require 'pp'

email = 'My Email'
password = 'My password'

latakoo = Latakoo.new();

## Get api list
print "Getting API List\n"
result = latakoo.systemApiList()
pp result

## Attempt to authenticate user
print "Authenticating a user\n"
begin
	details = latakoo.userAuthenticate(email, password)
	
	print "User has been authenticated, here are their details...\n"
	print details + "\n"
rescue LatakooFlightError => error
	print error
end

## Get my details
print "Obtaining user details\n"
begin
	me = latakoo.userGetById(latakoo.userid)
	print "That's you that is...\n";
	print me + "\n"
rescue LatakooFlightError => error
	print error
end