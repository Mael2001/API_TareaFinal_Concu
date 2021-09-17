"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    li.textContent = `${user} says ${message}`;
});

connection.start().then(function () {
    window.alert("Connection Established");
}).catch(function (err) {
    return console.error(err.toString());
});
