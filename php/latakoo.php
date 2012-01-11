<?php
	/**
	 * Latakoo flight library.
	 * @author Marcus Povey <marcus@marcus-povey.co.uk>
	 * @url http://www.marcus-povey.co.uk
	 * @url http://latakoo.com
	 */
	
	/**
	 * Latakoo flight class.
	 */
	class Latakoo 
	{
			private $email = '';
			private $password = '';
			private $token = '';
			private $userid = '';
			private $endpoint = 'http://latakoo.com/-/api/';
			
			/**
			 * Sign a query with a given token.
			 */
			protected static function sign($query, $email, $password, $token)
			{
				$query = trim($query);
				$email = trim($email);
				$token = trim($token);
				$password = trim($password);
				
				$now = time();
				
				return "$email:$now:" . md5(md5($password . $email) . $query . $now);
			}
			
			/**
			 * Execute an API call against the gateway.
			 * 
			 * @param string $method Method being called
			 * @param array $parameters Associated list of parameters in API call order.
			 * @param $call_details Additional call details, including:
			 *						'method' => 'GET' | 'POST'
			 * 						'headers' => Array of HTTP headers to send.
			 * 						'postdata' => If 'method' == POST then this is the data to send. 
			 * 						'token' => User authenticating token as retrieved by a call to
			 * 									Latakoo::authenticate('email','password');
			 * 						'email' => Email address of the signing user used in 'token'
			 * 						'password' => Password of the signing user used in 'token'
			 * 		
			 * @param url $endpoint The Endpoint to direct the query.
			 */
			public static function execute(
				$method,
				array $parameters = null, 
				array $call_details = null,
				$endpoint = 'latakoo.com'
			)
			{
				// Sanity check initial variables
				if (!$endpoint) return false;
				if (!$method) return false;
				if ((!$call_details) || (!is_array($call_details))) $call_details = array();
				if ((!$parameters) || (!is_array($parameters))) $parameters = array();
				
				if (!$call_details['method']) $call_details['method'] = 'GET';
				if (!$call_details['headers']) $call_details['headers'] = array();
				
				// Construct query string
				$params = array('method='.urlencode($method));
			
				foreach ($parameters as $k => $v) 
				{
					if (is_array($v)) 
					{
						foreach ($v as $v_k => $v_v)
							$params[] = urlencode($k)."[$v_k]=".urlencode($v_v);
					} else
						$params[] = urlencode($k).'='.urlencode($v);
				}
				
				$query = implode('&', $params);
				$endpoint = "https://$endpoint/-/api/";
				$endpoint .= '?' . $query;
				
				// Authenticate command
					$token = self::sign($query, $call_details['email'], $call_details['password'], $call_details['token']);
				
				if ($token)
					$call_details['headers'] += array("X_PFTP_API_TOKEN: $token");
					
				
				// Construct stream
				$http = array (
					'method' => strtoupper($call_details['method']),
					'header' => implode("\r\n", $call_details['headers']) . "\r\n"
				);
				if (strtoupper($call_details['method'])=='POST') 
					$http['content'] = $call_details['postdata'];
					
				// Execute query
				$ctx = stream_context_create(array(
					'http' => $http 
				));
				
				if ($fp = @fopen($endpoint, 'rb', false, $ctx)) {
					$response = @stream_get_contents($fp);
					fclose($fp);
				} 
				
				if ($response)
				{
					if ($decode = json_decode($response))
						return $decode;
				} 
				
				return false;
			}
			
			public function getEndpoint() { return $this->endpoint; }
			
			
			
			public function __construct($endpoint = 'latakoo.com')
			{
				$this->endpoint = trim($endpoint);
			}
			
			/**
			 * List the API and parameters provided by the endpoint.
			 */
			public function systemApiList()
			{
				return self::execute('system.api.list', null, null, $this->getEndpoint());
			}
			
			/**
			 * Authenticate a user against the latakoo Flight API.
			 * All further commands will be executed as the authenticated user.
			 */
			public function userAuthenticate($email, $password)
			{
				$email = trim($email);
				$password = trim($password);
				
				$result = self::execute(
					'user.authenticate.x',
					 array('email' => $email, 'password' => $password)
				);
				
				if ($result)
				{
					if ($result->status_code == 0) 
					{
						
						// Extract token from result
						$authcode = explode(':', $result->result->authcode);
						
						if ($authcode)
						{
							$this->token = $authcode[0];
							$this->userid = $authcode[1];

							return $result;
						}
					}
					else
						throw new LatakooFlightException($result->message);
				}
				
				return false;
			}
			
		
	}

	/**
	 * Client exceptions.
	 */
	class LatakooFlightException extends Exception {};
