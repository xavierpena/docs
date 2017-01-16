# How to pass states between pages

## Introduction

I want to use javascript only.

The goal is to go from **Page A** (which holds some data) to **Page B**, and then return to **Page A** without loosing the data it held.

What we'll do:
* When calling **Page B** (from **Page A**), we'll add a parameter `state=SOME_DATA` to the URL.
* **Page B** don't need to decode this parameter, just keep it and add it to the URL if the user decides to go back to **Page A**.
* If we go back to **Page A**, it will read the parameter `state=SOME_DATA` so it can come back to the same state it had in the begining.


## Encoding

For encoding, I will use [jQuery.param](http://api.jquery.com/jQuery.param/). From the demo:

	var myObject = {
	  a: {
		one: 1,
		two: 2,
		three: 3
	  },
	  b: [ 1, 2, 3 ]
	};
	
	var recursiveEncoded = $.param( myObject );
	 
	console.log( recursiveEncoded );
	// shows: a%5Bone%5D=1&a%5Btwo%5D=2&a%5Bthree%5D=3&b%5B%5D=1&b%5B%5D=2&b%5B%5D=3 
	

## Decoding
	
To decode the json from the URL, I will use [this function](http://stackoverflow.com/a/3401265/831138):

	function QueryStringToHash(query) {

	  if (query == '') return null;

	  var hash = {};

	  var vars = query.split("&");

	  for (var i = 0; i < vars.length; i++) {
		var pair = vars[i].split("=");
		var k = decodeURIComponent(pair[0]);
		var v = decodeURIComponent(pair[1]);

		// If it is the first entry with this name
		if (typeof hash[k] === "undefined") {

		  if (k.substr(k.length-2) != '[]')  // not end with []. cannot use negative index as IE doesn't understand it
			hash[k] = v;
		  else
			hash[k.substr(0, k.length-2)] = [v];

		// If subsequent entry with this name and not array
		} else if (typeof hash[k] === "string") {
		  hash[k] = v;  // replace it

		// If subsequent entry with this name and is array
		} else {
		  hash[k.substr(0, k.length-2)].push(v);
		}
	  } 
	  return hash;
	};
	
## Testing both

Yo can go to http://plnkr.co and write:

	<script src="https://code.jquery.com/jquery-3.1.1.min.js"></script>
    
	<script>

		$(document).ready(function() {
		  
			var myObject = {
			  a: {
				one: 1,
				two: 2,
				three: 3
			  },
			  b: [ 1, 2, 3 ]
			};
			
			var recursiveEncoded = $.param( myObject );
			 
			console.log( recursiveEncoded );
			// shows: a%5Bone%5D=1&a%5Btwo%5D=2&a%5Bthree%5D=3&b%5B%5D=1&b%5B%5D=2&b%5B%5D=3 
			
			var decoded = QueryStringToHash(recursiveEncoded);
			console.log(decoded);
			
			function QueryStringToHash(query) {

			  if (query == '') return null;

			  var hash = {};

			  var vars = query.split("&");

			  for (var i = 0; i < vars.length; i++) {
				var pair = vars[i].split("=");
				var k = decodeURIComponent(pair[0]);
				var v = decodeURIComponent(pair[1]);

				// If it is the first entry with this name
				if (typeof hash[k] === "undefined") {

				  if (k.substr(k.length-2) != '[]')  // not end with []. cannot use negative index as IE doesn't understand it
					hash[k] = v;
				  else
					hash[k.substr(0, k.length-2)] = [v];

				// If subsequent entry with this name and not array
				} else if (typeof hash[k] === "string") {
				  hash[k] = v;  // replace it

				// If subsequent entry with this name and is array
				} else {
				  hash[k.substr(0, k.length-2)].push(v);
				}
			  } 
			  return hash;
			};    	
			
		});

	</script>

 