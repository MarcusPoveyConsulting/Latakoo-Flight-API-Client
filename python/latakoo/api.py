import time
import hashlib
import urllib
import httplib
import json

class Latakoo:
    email = ''
    password = ''
    token = ''
    userid = ''
    endpoint = ''
    
    def __init__(self, endpoint = 'latakoo.com'):
        self.email = ''
        self.password = ''
        self.token = ''
        self.userid = ''
        self.endpoint = endpoint
       
    #
    # Sign a query with a given token.
    # 
    def sign(self, query, email, password, token):
        query = query.strip()
        email = email.strip()
        password = password.strip()
        token = token.strip()
        
        now = int(time.time())
        now = str(now)
        
        pwsalt = hashlib.md5(password + token).hexdigest()
        
        hash = email + ':' + now + ':' + hashlib.md5(pwsalt + query + now).hexdigest()
        
        return hash
    
    #
	# Execute an API call against the gateway.
	# 
	# @param string $method Method being called
	# @param array $parameters Associated list of parameters in API call order.
	# @param $call_details Additional call details, including:
	#						'method' => 'GET' | 'POST'
	# 						'headers' => Array of HTTP headers to send.
	# 						'postdata' => If 'method' == POST then this is the data to send. 
	# 						'token' => User authenticating token as retrieved by a call to
	# 									Latakoo::authenticate('email','password');
	# 						'email' => Email address of the signing user used in 'token'
	# 						'password' => Password of the signing user used in 'token'
	# 		
	# @param url $endpoint The Endpoint to direct the query.
	#
    def execute(self, method, parameters = {}, call_details = {'method':'GET'}, endpoint = 'latakoo.com'):
        
        # Sanity check
        method = method.strip()
        if not isinstance(parameters, dict): 
            parameters = {}
        if not isinstance(call_details, dict): 
            call_details = {}
        if not 'method' in call_details:
            call_details['method'] = 'GET'
        if not 'headers' in call_details:
            call_details['headers'] = {}
        
                
        # Construct query string
        params = ['method=' + urllib.quote(method)]
        
        for key, value in parameters.iteritems():
            if isinstance(value, dict):
                for v_key, v_value in value.interitems():
                    params.append(urllib.quote(v_key) + '['+v_key+']=' +urllib.quote(v_v))
            else:
                params.append(urllib.quote(key) + '=' +urllib.quote(value))

        query = '&'.join(params)
        
        # Authenticate command
        if ('token' in call_details) and ('email' in call_details) and ('password' in call_details):
            token = self.sign(query, call_details['email'], call_details['password'], call_details['token'])
            call_details['headers']['X_PFTP_API_TOKEN'] = token
            
        # Execute command
        conn = httplib.HTTPSConnection(endpoint)
        if call_details['method'] == 'POST':
            conn.request('POST', '/-/api/?' + query, call_details['postdata'], call_details['headers'])
        else:
            conn.request('GET', '/-/api/?' + query, None, call_details['headers'])
                        
        response = conn.getresponse()
        data = response.read()
        conn.close()
        
        return json.loads(data)
        