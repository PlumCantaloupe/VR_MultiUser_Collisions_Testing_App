const express   = require('express');
const app       = express();
const http      = require('http');
const fs        = require('fs');
const url       = require('url');
const server    = require('http').createServer(app);  
const io        = require('socket.io')(server);

//MAX 8 Users .... for now
const LISTEN_PORT   = 8080;
const MAX_USERS     = 8;

//!!want to load in JSOn object representing data we constantly send
function getUserObj(id, x, y, z, r, g, b, message)
{
    let userObj = {};
    userObj.id = id;
    userObj.x = x;
    userObj.y = y;
    userObj.z = z;
    userObj.r = r;
    userObj.g = g;
    userObj.b = b;
    userObj.message = message;
    return userObj;
}

//8 color
let colorPalette = [
    {name: "turquoise", r:2, g:191, b:155, beingUsed:false},
    {name: "emerald", r:33, g:211, b:105, beingUsed:false},
    {name: "river", r:43, g:146, b:223, beingUsed:false},
    {name: "amethyst", r:155, g:71, b:186, beingUsed:false},
    {name: "asphalt", r:51, g:73, b:96, beingUsed:false},
    {name: "sunflower", r:243, g:201, b:3, beingUsed:false},
    {name: "carrot", r:233, g:126, b:1, beingUsed:false},
    {name: "mandarin", r:233, g:65, b:46, beingUsed:false}
];

server.listen(LISTEN_PORT);
app.use(express.static(__dirname + '/public')); //set root path of server ...

console.log("Listening on port: " + LISTEN_PORT );

app.get( '/', function( req, res ){ 
    res.sendFile( __dirname + '/public/user.html' );
});

app.get( '/user', function( req, res ){ 
    res.sendFile( __dirname + '/public/user.html' );
});

app.get( '/controller', function( req, res ){ 
    res.sendFile( __dirname + '/public/controller.html' );
});

// app.get( '/*' , function( req, res, next ) {

//             //This is the current file they have requested
//         var file = req.params[0];
 
//             //For debugging, we can track what files are requested.
//         if(verbose) console.log('\t :: Express :: file requested : ' + file);

//             //Send the requesting client the file.
//         res.sendfile( __dirname + '/' + file );

//     });

// // Loading the index file . html displayed to the client
// const server = http.createServer(function(req, res) {
//     var path = url.parse(req.url).pathname;

//     fs.readFile(__dirname + path, function(error, data){
//         if (error){
//             res.writeHead(404);
//             res.write("opps this doesn't exist - 404");
//             res.end();
//         }
//         else{
//             const fileExt = path.split('.').pop();

//             //console.log("ext:" + fileExt );

//             if ( fileExt === "html" ) {
//                 res.writeHead(200, {"Content-Type": "text/html"});
//             }
//             else if ( fileExt == "css"  ) {
//                 res.writeHead(200, {"Content-Type": "text/css"});
//             }
//             else if ( fileExt == "js"  ) {
//                 res.writeHead(200, {"Content-Type": "application/javascript"});
//             }
//             else if ( fileExt == "json"  ) {
//                 res.writeHead(200, {"Content-Type": "application/json"});
//             }
//             else {
//                 res.writeHead(200, {"Content-Type": "text/html"});
//             }

//             res.write(data, "utf8");
//             res.end();
//         }
//     });
// });

let setIntervalFunc     = null;
const dataSend_Interval = 50; //ms
let users               = new Array();

//do I need this??
io.use(function(socket, next) {
  //var handshakeData = socket.request;
  // make sure the handshake data looks good as before
  // if error do this:
    // next(new Error('not authorized');
  // else just call next
  next();
});

//namespace_controller.on('connection', function (socket) {
io.on('connection', (socket) => {

    socket.on('disconnect', () => {
        console.log( socket.id + " disconnected" );
        removeUser(socket.id);

        //need to send disconnect event here
    });

    socket.on("newUser", () => {
        if ( users.length == MAX_USERS ) {
            console.log("reached max users");
        }
        else {
            const userCol = selectNewColor();
            let userObj = getUserObj(socket.id, 0.0, 0.0, 0.0, userCol.r, userCol.g, userCol.b, ""); //use socket is for UUID ....
            io.emit("user_connect", userObj);
            users.push( userObj ); //add new empty user with correct ID. We will update position later

            console.log( "newUser received " + socket.id );
        }
    });

    socket.on("newController", (data) => {
        //console.log("message: " + data.message);
    });

    socket.on("posUpdate", (data) => {
        //console.log("updating user position");
        updateUserPosition(data.id, data.x, data.y, data.z);
    });

    //infinite loop with a millisecond delay (but only want one loop running ...)
    if (!setIntervalFunc) {
        console.log("setting interval func");
        setIntervalFunc = setInterval( () => {
            console.log("looping ....");

            //if removed users makes numUsers == 0 may as well stop/clear loop
            if (users.length === 0) {
                clearInterval(setIntervalFunc);
                setIntervalFunc = null;
            }

            io.sockets.emit("usersData", {items: users}); //needs to be named items (within an object) to be readable within Unity currently ...

        }, dataSend_Interval);
    }
});

//custom functions
function updateUserPosition( id, xPos, yPos, zPos )
{
    for ( let i = 0; i < users.length; i++ ) {
        let element = users[i];
        if ( id === element.id ) {
            element.x = xPos;
            element.y = yPos;
            element.z = zPos;
            break;
        }
    }
}

function removeUser( id )
{
    if ( users.length > 0 ) {
        for ( let i = users.length-1; i >= 0; i-- ) {
            let element = users[i];
            if ( id === element.id ) {
                //make color available
                for ( let j = colorPalette.length-1; j >= 0; j-- ) {
                    let color = colorPalette[j];
                    if (    element.r === color.r &&
                            element.g === color.g &&
                            element.b === color.b ) {
                        color.beingUsed = false;
                    }
                }
                users.splice(i, 1); //remove user from positions array
                break;
            }
        }
    }
}

function selectNewColor()
{
    let colorFound = false;
    while (!colorFound) {
        let randIndex = Math.floor(Math.random() * (colorPalette.length-1)) + 0;
        let randCol   = colorPalette[randIndex]; 
        if (randCol.beingUsed == false) {
            randCol.beingUsed = true;
            return randCol;
        }
    }
}
