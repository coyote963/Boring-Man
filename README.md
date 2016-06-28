# Boring-Man
Sets up a boring man server with a bunch of improvements

Instructions
Download Mono
http://www.mono-project.com/download/

Download my app
http://www.filehosting.org/file/details/580134/app.7z
unzip with 7zip
put files somewhere you will remember


Open the bm_settings.ini
Change RCon to ON
Change Dedicated to ON
Change RCon Password to something you will remember

Open BMConsoleTester.exe with text editor
Change the values in BMValues
Some Notes:
Winner Text is just capitalized team name followed by WINS
(ex. if Man has been renamed to "RED" MAN_WINNER_TEXT is "RED WINS")
Add your public IPAddress 
Change RConPassword to what you set it as
If your port isn't 7778 change that too.

Compile the BMConsoleTester
Open cmd prompt (ctrl + r ... cmd ... enter)
navigate to the folder you put BMConsoleTester in. Shift click on the folder and click open command prompt here.
Type mcs BMConsoleTester
Type mono BMConsoleTester

Change Chat_list using this format
input|output
