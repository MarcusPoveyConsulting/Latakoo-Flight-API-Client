###
# Latakoo Flight API Library (Ruby)
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
require 'digest/md5'
require 'uri'
require 'net/http'
require 'net/https'
require 'rubygems'
require 'json'

class Latakoo

	attr_accessor :email, :password, :token, :userid, :endpoint
		
	def initialize(endpoint = "latakoo.com")
		@endpoint = endpoint
	end
	
	##
	# Sign a query with a given token.
	##
	def sign(query, email, password, token)
		query = query.strip()
		email = email.strip()
		password = password.strip()
		token = token.strip()
		
		inow = Time.now.to_i
		now = inow.to_s()
			
		pwtok = Digest::MD5.hexdigest(password + token)
		
		return "#{email}:#{now}:" + Digest::MD5.hexdigest(pwtok + "#{query}#{now}") 
	end
	
	##
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
	##
	def execute(method, parameters = {}, call_details = {"method" => "GET"}, endpoint = "latakoo.com")
		
		# Sanity checks
		method = method.strip()
		
		if call_details == nil
			call_details = {}
		end
		
		if parameters == nil
			parameters = {}
		end
		
		if !call_details.has_key?("method") 
			call_details["method"] = "GET"
		end
		
		if !call_details.has_key?("headers")
			call_details["headers"] = {}
		end
		
		# Construct query string
		params = []
		params << "method=" + URI.escape(method)
		parameters.each do | key, value |
			
			if value.instance_of? Hash
				value.each do | v_key, v_value |
					params << URI.escape(key) + "=[" +URI.escape(v_key)+ "]" + URI.escape(v_value)
				end
			else
				params << URI.escape(key) + "=" + URI.escape(value)
			end
			
		end
		
		query = params.join("&")
		endpoint = "https://#{endpoint}/-/api/?#{query}"
		
		# Authenticate command
		if call_details.has_key?("email") and call_details.has_key?("password") and call_details.has_key?("token")
			token = sign(query, call_details["email"], call_details["password"], call_details["token"])
			call_details["headers"]["X_PFTP_API_TOKEN"] = token
		end
		
		# Construct stream
		uri = URI.parse(endpoint)
		
		http = Net::HTTP.new(uri.host, uri.port)
		http.use_ssl = true
		http.verify_mode = OpenSSL::SSL::VERIFY_NONE
		
		request = Net::HTTP::Get.new(uri.request_uri)
		if call_details["method"] == "POST"
			request = Net::HTTP::Post.new(uri.request_uri)
			request.body = call_details["postdata"]
		end
		
		# Add headers
		call_details["headers"].each do | hkey, hval |
			request[hkey] = hval
		end
		
		response = http.request(request)
		
		return JSON.parse(response.body)

	end
	
	##
	# List the API and parameters provided by the endpoint.
	##
	def systemApiList()
		return execute("system.api.list", nil, nil, @endpoint)
	end
	
	##
	# Authenticate a user against the latakoo Flight API.
	# All further commands will be executed as the authenticated user.
	##
	def userAuthenticate(email, password)
		email = email.strip()
		password = password.strip()
		
		result = execute("user.authenticate.x", {"email" => email, "password" => password}, nil, @endpoint)
		if (result['status_code'] == 0)
			authcode = result["result"]["authcode"]
			bits = authcode.split(":")
			
			if bits.count == 2
				@token = bits[0]
				@userid = bits[1]
				@email = email
				@password = password
				
				return JSON(result["result"])
				
			end
			
		else
			raise LatakooFlightError, result['message']
		end
		
	end
	
	##
	# Return the details of a specific user.
	##
	def userGetById(id)
		result = execute("user.get.byid", {"id" => id}, {"email" => @email, "password" => @password, "token" => @token}, @endpoint)
		
		if (result['status_code'] == 0)
			return JSON(result['result'])
		else
			raise LatakooFlightError, result['message']
		end
	end
	
end

class LatakooFlightError < StandardError

end