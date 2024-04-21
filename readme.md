# CurlHttpParser

Convert valid curl text to `HttpRequestMessage` object



## Example
sample curl string:

     curl -X "GET" "https://api.sendgrid.com/v3/templates" -H "Authorization: Bearer Your.API.Key-HERE" -H     "Content-Type: application/json"



load curl string into a string variable and execute request.
### Code
            using CurlHttpParser; 
            ...
	    
            StringParser parser = new StringParser();
            var message = parser.CreateHttpRequest(requestString);
            
            using(var client  = new HttpClient()){
	            client.SendAsync(message);
            }
            

