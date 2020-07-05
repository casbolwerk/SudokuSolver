# SudokuSolver
This is as sudoku solver using chronological backtracking and forward checking in C#.

The code contains comments, a lot of which are in Dutch. Sorry for the inconvenience.

When the project is forked/downloaded, it should be used by using the developer command
prompt of Visual Studio. It can also be used by using the regular cmd, but then
csc (or a lack thereof) can cause errors.



So open the developer command prompt for Visual Studio by searching for it on your pc.
Then change the directory to the project folder. Then follow these guidelines.

## Command Lines Guidelines

The commands are based on sudoku_puzzels_9x9.txt's layout.
The layout is as follows:
	
	Grid X
	[SUDOKU]
	Grid X+1
	[SUDOKU]
	etc.

Where SUDOKU is N lines of length N without whitespace
in between.

Runs on text files can be called by having the text file
in the project folder, opening the cmd in this folder
and calling the following command:

	program.exe -CB/-FC -sud filename.txt

When changes are made to the code and you want to
run the commands with this new code, the following
line should work. It can work in cmd if you have added csc
to the path of your computer, or in the developer command
prompt of Visual Studio (with the directory being the
project folder) without needing the csc path.

	csc.exe program.cs

This should update the .exe file of program.cs.
It could be that this line doesn't work, if your csc
path isn't yet added to the path of your computer.
