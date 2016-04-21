# chat
simple chat app built using BigQ

## description
Download and run the client app.  

If you wish to use our server (be warned - it's unmoderated!) you can, just point your client to:

```
chat.bigq.io 
port 8222
```

To make life easy, we've set the client up to use your handle as your GUID.  While you can log in using a handle that's already in use, please don't.  It's not like you'll break anything, but we made this simplification to show how easy it is to get startd.  Obviously in production you'll use GUIDs.

## performance
bigq is still early in development.  While we have high aspirations on performance, it's not there yet.  The software has excellent stability in lower throughput environments with lower rates of network change (adds, removes).  Performance will be a focus area in the coming releases.
