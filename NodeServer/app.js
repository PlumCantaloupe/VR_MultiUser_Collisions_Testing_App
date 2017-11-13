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

//just some consts for typed array access
const R = 0;
const G = 1;
const B = 2;

const X = 0;
const Y = 1;
const Z = 2;
const W = 3;

function getUserObj(id,

            r, g, b,

            root_pos_x, root_pos_y, root_pos_z,
            head_pos_x, head_pos_y, head_pos_z,
            hand_L_pos_x, hand_L_pos_y, hand_L_pos_z,
            hand_R_pos_x, hand_R_pos_y, hand_R_pos_z,

            root_rot_w, root_rot_x, root_rot_y, root_rot_z,
            head_rot_w, head_rot_x, head_rot_y, head_rot_z,
            hand_L_rot_w, hand_L_rot_x, hand_L_rot_y, hand_L_rot_z,
            hand_R_rot_w, hand_R_rot_x, hand_R_rot_y, hand_R_rot_z,

            message) {
    let userObj = {};

    userObj.id = id;
    userObj.color = [r, g, b];

    userObj.pos_root = [root_pos_x, root_pos_y, root_pos_z];
    userObj.pos_head = [head_pos_x, head_pos_y, head_pos_z];
    userObj.pos_hand_L = [hand_L_pos_x, hand_L_pos_y, hand_L_pos_z];
    userObj.pos_hand_R = [hand_R_pos_x, hand_R_pos_y, hand_R_pos_z];

    userObj.rot_root = [root_rot_x, root_rot_y, root_rot_z, root_rot_w];
    userObj.rot_head = [head_rot_x, head_rot_y, head_rot_z, head_rot_w];
    userObj.rot_hand_L = [hand_L_rot_x, hand_L_rot_y, hand_L_rot_z, hand_L_rot_w];
    userObj.rot_hand_R = [hand_R_rot_x, hand_R_rot_y, hand_R_rot_z, hand_L_rot_w];

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
            let userObj = getUserObj(   socket.id,
                                        userCol.r, userCol.g, userCol.b,
                                        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                        ""); //use socket is for UUID ....
            socket.emit("user_connect", userObj);
            users.push( userObj ); //add new empty user with correct ID. We will update position later

            console.log( "newUser received " + socket.id );
        }
    });

    socket.on("newController", (userData) => {
        //console.log("message: " + data.message);
    });

    //socket.on("posUpdate", (userData) => {
    socket.on("transformUpdate", (userData) => {
        //console.log("updating user position");
        updateUserTransformations(userData);
    });

    //infinite loop with a millisecond delay (but only want one loop running ...)
    if (setIntervalFunc != null) {
        console.log("setting interval func");
        setIntervalFunc = setInterval( () => {
            //console.log("looping ....");

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
function updateUserTransformations( userData )
{
    for ( let i = 0; i < users.length; i++ ) {
        let element = users[i];
        if (userData.id === element.id) {
            element.pos_root    = [...userData.pos_root];
            element.pos_head    = [...userData.pos_head];
            element.pos_hand_L  = [...userData.pos_hand_L];
            element.pos_hand_R  = [...userData.pos_hand_R];

            element.rot_root    = [...userData.rot_root];
            element.rot_head    = [...userData.rot_head];
            element.rot_hand_L  = [...userData.rot_hand_L];
            element.rot_hand_R  = [...userData.rot_hand_R];
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
                    if (    element.color[R] === color.r &&
                            element.color[G] === color.g &&
                            element.color[B] === color.b ) {
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
