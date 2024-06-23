const express = require('express');
const http = require('http');
const socketIo = require('socket.io');
const mqtt = require('mqtt');

const app = express();
const server = http.createServer(app);
const io = socketIo(server);

const mqttClient = mqtt.connect('mqtt://mosquitto:1883');
const mqttTopic1 = 'Alert';
const mqttTopic2 = 'AlertTemp';

let messageTopic1 = {};
let messageTopic2 = {};

mqttClient.on('connect', () => {
    console.log('Connected to MQTT broker');
    mqttClient.subscribe(mqttTopic1, (err) => {
        if (err) {
            console.error('Failed to subscribe to topic:', mqttTopic1);
        } else {
            console.log('Subscribed to topic:', mqttTopic1);
        }
    });
    mqttClient.subscribe(mqttTopic2, (err) => {
        if (err) {
            console.error('Failed to subscribe to topic:', mqttTopic2);
        } else {
            console.log('Subscribed to topic:', mqttTopic2);
        }
    });
});

mqttClient.on('message', (topic, message) => {
    const parsedMessage = JSON.parse(message.toString());
    console.log(`Received message from ${topic}:`, parsedMessage);

    if (topic === mqttTopic1) {
        messageTopic1 = parsedMessage;
        io.emit('mqtt_message_topic1', messageTopic1);
    } else if (topic === mqttTopic2) {
        messageTopic2 = parsedMessage;
        io.emit('mqtt_message_topic2', messageTopic2);
    }
});

io.on('connection', (socket) => {
    console.log('New client connected');
    

    socket.on('disconnect', () => {
        console.log('Client disconnected');
    });
});

app.use(express.static('public'));

const PORT = 5001;
server.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});
