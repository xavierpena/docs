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
	
To decode the json from the URL, I will use [this function](https://github.com/chrissrogers/jquery-deparam) extracted from [jQuery BBQ](http://benalman.com/projects/jquery-bbq-plugin/). The documentation for the deparam function can be found [here](http://benalman.com/code/projects/jquery-bbq/examples/deparam/).

	
## Testing both together

Yo can go to http://plnkr.co and write this (remember to copy and paste the deparam function first):

	<script src="https://code.jquery.com/jquery-3.1.1.min.js"></script>
    
	<script>
	
		// !!!
		// Copy and paste here the content of the following URL (containing the extracted version of the function `deparam`):
		// https://raw.githubusercontent.com/chrissrogers/jquery-deparam/master/jquery-deparam.js

		$(document).ready(function() {
		  
		  	// TEST OBJECT:
			var myObject = {
			  a: {
				one: 1,
				two: 2,
				three: 3
			  },
			  b: [ 1, 2, 3 ]
			};
			
			// ENCODE:
			var recursiveEncoded = $.param( myObject );			 
			console.log( recursiveEncoded );
			// result: a%5Bone%5D=1&a%5Btwo%5D=2&a%5Bthree%5D=3&b%5B%5D=1&b%5B%5D=2&b%5B%5D=3 
			
			// DECODE:
			var decoded = $.deparam(recursiveEncoded);			
			console.log(decoded);
			// result: Object {a: Object, b: Array[3]}
									
		});

	</script>
	

## How to use it (A->B->A)

In page A, before sendig the user to B:

	// Add the encoded state in the URL to page B:
	var myState = { ... }; // ***
	var encodedState = $.param( myState );
	var urlToPageB = 'http://url/to/page/B?' + encodedState;
	

In page B, get the encoded parameters (no need to decode):

	// Read the part after "?" in the URL:
	var encodedState = document.location.search.substring(1);
	var urlBackToPageA = 'http://url/to/page/B?' + encodedState;
	
	
Back to page A, get the encoded parameters and decode them:
	
	// Decode it:
	var encodedState = document.location.search.substring(1);
	var decodedState = $.deparam(encodedState);
	// voilà! you can use decodedState, which will be the same as step 1 (***).
