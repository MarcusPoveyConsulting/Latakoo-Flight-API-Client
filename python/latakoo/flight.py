###
# Latakoo Flight API Library (Python)
# 
# @author Marcus Povey <marcus@marcus-povey.co.uk>
# @copyright Marcus Povey 2012
# @link http://www.marcus-povey.co.uk
# @link https://github.com/mapkyca/Latakoo-Flight-API-Client
# @link http://latakoo.com
# 
# Copyright (c) 2011-12 Marcus Povey
#
# Permission is hereby granted, free of charge, to any person obtaining a copy of
# this software and associated documentation files (the "Software"), to deal in
# the Software without restriction, including without limitation the rights to
# use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
# the Software, and to permit persons to whom the Software is furnished to do so,
# subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
# FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
# COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
# IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
# CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
###
from api import Latakoo

class LatakooFlightException(Exception):
    pass

class LatakooFlight(Latakoo):
    def systemApiList(self):
        return self.execute('system.api.list')
    
    
    def userAuthenticate(self, email, password):
        
        # Sanity check
        email = email.strip()
        password = password.strip()
        
        result = self.execute('user.authenticate.x', {'email':email, 'password':password})
        
        if result:
            if result['status_code'] == 0:
                authcode = result['result']['authcode'].split(':')
                
                if authcode:
                    self.token = authcode[0]
                    self.userid = authcode[1]
                    self.email = email
                    self.password = password
                    
                    return result['result']

            else:
                raise LatakooFlightException(result['message'])
            
        return false
    
    
    def userGetById(self, id):

        result = self.execute('user.get.byid', {'id':id}, {'email': self.email, 'password': self.password, 'token': self.token})
        
        if result:
            if result['status_code'] == 0:
                return result['result']
            else:
                raise LatakooFlightException(result['message'])
            
        return false
