# Serial Mouse Parser
A simple program that parses the signal from a Logitech M-M35 3-button serial mouse.

![Screenshot](https://github.com/Blamoo/SerialMouseParser/blob/master/resources/demo.png?raw=true)

# Bugs
Since the packets have 3 (Microsoft Standard) or 4 (Logitech 3-button extension) bytes, i can only "finish" a packet when the first byte of a new packet (X1XXXXXX) is sent. Feel free to suggest another method for parsing the data.

# References
 - http://linux.die.net/man/4/mouse
