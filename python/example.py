#!/usr/bin/python

import latakoo.api
import latakoo.flight
from latakoo.flight import LatakooFlight
from latakoo.flight import LatakooFlightException


email = 'my email addess'
password = 'my password'

# Create interface
koo = LatakooFlight()

# Retrieve api list
print koo.systemApiList()


# Authenticate a user
try:
    details = koo.userAuthenticate(email, password)
    if details:
        print "User has been authenticated, here are their details..."
        print details
except LatakooFlightException as error:
    print error
    
# Get my details
try:
    details = koo.userGetById(koo.userid)
    if details:
        print "That's you that is..."
        print details
except LatakooFlightException as error:
    print error