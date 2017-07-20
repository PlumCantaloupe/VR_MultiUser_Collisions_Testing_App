const express = require('express');
const app = express();
const http = require('http');
const fs = require('fs');
const url = require('url');

//MAX 8 Users .... for now
const MAX_USERS = 8;

//want to load in JSOn object representing data we constantly send
var userObj_proto = JSON.parse(fs.readFileSync(__dirname + '/UserObject.json', 'utf8'));

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

// Loading the index file . html displayed to the client
var server = http.createServer(function(req, res) {
    var path = url.parse(req.url).pathname;

    fs.readFile(__dirname + path, function(error, data){
        if (error){
            res.writeHead(404);
            res.write("opps this doesn't exist - 404");
            res.end();
        }
        else{
            res.writeHead(200, {"Content-Type": "text/html"});
            res.write(data, "utf8");
            res.end();
        }
    });
});

const io = require('socket.io').listen(server);
let setIntervalFunc     = null;
const dataSend_Interval = 3000; //ms
let allPositions        = new Array();

//namespace_controller.on('connection', function (socket) {
io.on('connection', function (socket) {
    socket.on('disconnect', function(){
        console.log( socket.id + " disconnected" );
        removeUser(socket.id);
    });

    socket.on("newUser", (data) => {
        console.log( "newUser received " + socket.id );
        //console.log("message: " + data.message);
        if ( allPositions.length == MAX_USERS ) {
            console.log("reached max users");
        }
        else {
            const userCol = selectNewColor();
            let userObj = userObjCopy(); //create new user obj
            userObj.socketID = socket.id;
            userObj.r        = userCol.r;
            userObj.g        = userCol.g;
            userObj.b        = userCol.b;
            socket.emit("givenID", userObj);
            allPositions.push( userObj ); //add new empty user with correct ID. We will update position later
        }
    });

    socket.on("newController", (data) => {
        //console.log("message: " + data.message);
    });

    socket.on("positionUpdate", (data) => {
        console.log("updating user position");
        updateUserPosition(data.socketID, data.x, data.y, data.z);
    });

    //infinite loop with a millisecond delay (but only want one loop running ...)
    if (!setIntervalFunc) {
        console.log("setting interval func");
        setIntervalFunc =   setInterval( () => {
            console.log("looping ....");

            //if removed users makes numUsers == 0 may as well stop/clear loop
            if (allPositions.length === 0) {
                clearInterval(setIntervalFunc);
                setIntervalFunc = null;
            }

            io.sockets.emit("usersData", {items: allPositions}); //needs to be named items (within an object) to be readable within Unity currently ...

        }, dataSend_Interval);
    }
});

server.listen(8080);

//custom functions
function updateUserPosition( socketID, xPos, yPos, zPos )
{
    for ( let i = 0; i < allPositions.length; i++ ) {
        let element = allPositions[i];
        if ( socketID === element.socketID ) {
            element.x = xPos;
            element.y = yPos;
            element.z = zPos;
            break;
        }
    }
}

function removeUser( socketID )
{
    if ( allPositions.length > 0 ) {
        for ( let i = allPositions.length-1; i >= 0; i-- ) {
            let element = allPositions[i];
            if ( socketID === element.socketID ) {
                //make color available
                for ( let j = colorPalette.length-1; j >= 0; j-- ) {
                    let color = colorPalette[j];
                    if (    element.r === color.r &&
                            element.g === color.g &&
                            element.b === color.b ) {
                        color.beingUsed = false;
                    }
                }
                allPositions.splice(i, 1); //remove user from positions array
                break;
            }
        }
    }
}

function userObjCopy()
{
    return JSON.parse( JSON.stringify(userObj_proto) );
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
