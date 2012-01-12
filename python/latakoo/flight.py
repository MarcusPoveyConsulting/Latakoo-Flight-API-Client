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
