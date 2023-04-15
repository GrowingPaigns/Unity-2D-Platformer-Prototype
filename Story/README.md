#### Title: CSCI 3762 - Network Programming - Lab 7

#### Name: Samuel Hilfer

#### Due: 04/17/2023

## Movement and Optimization - Lab 7

### To-Do:				

| Requrements                                                               | Completed |
|---------------------------------------------------------------------------|-----------|
| Lab 6 (and prior) functionality                                           | √         |
| 'move' variable and functionality                                         | √         |
| Optimized send path (msgs not forwarded to any ports in send path)        | √         |

## Code updates from the previous lab...
	
      The most notable update to the code in this lab is the concept of movement being implemented. Now with a single command, no matter how far away a drone is located in the "grid" from the sender, the drone will change its position (location). This section was rather simple to implement, and even helped me refine my code by now storing every config related variable in a struct, making it much easier to modify drone values than it was before.  

      Beyond that, in this lab we also optimized the way in which messages get forwarded. Now, everytime a message gets forwarded, the current ports in the send-path kvp are checked, and any ports within that path which also match our stored config variable get skipped over. This basically ensures that duplicate messages are no longer received regardless of the TTL. The drone/drones which wind up forwarding messages also inform the user of which ports they will not be forwarding the message to, though, I did exclude the port of the forwarding machine from these messages, as I believe its assumed that the forwarding drone would not be sending to itself.  

---------------------------------------------------------------------------

### [NOTES FOR RUNTIME]
	
      I have not changed anything regarding runtime between this and the previous lab. The executed 'make' command is still 'gcc -o drone7 drone7.c -lm', and I have not changed any of the variables the user needs to enter on the command line. The 'msg' and 'toPort' kvps are requred to be input before each send, and the 'move' command is optional.
         
        ---------------------------------------------------------------------------
                 {Everything below this line is runtime notes from lab 6}
        ---------------------------------------------------------------------------
        
      With version 6 of the drone, I have run into an issue with my makefile. Though the commands I am using are technically correct, and though the same makefile worked with lab 5 (drone5), there is now an error that pops up when running "make" that states that one of my variables is set but never used. This may sound like a simple issue, but its not. That's because this variable is absolutely being used in multiple spots throughout the function it is defined within. I was unable to figure out why exactly the make command is producing this error, though I was able to narrow the issue down to the CFLAGs (either -g or -Wall) being the issue. This is quite confusing to me, because previously in the semester I was using those two flags to make my program without any errors.
    
    My solution to this was to simply remove the CFLAGS, which means my makefile now runs without them, but it also makes properly. For easy reference, below are the two commands that are/were being used by the makefile.
    
        Old:
            gcc -g -Wall drone6.c -o drone6 -lm
    
    
        New:
            gcc -o drone6 drone6.c -lm
         

---------------------------------------------------------------------------


### Issues during testing on (04/17/2023):

    (TBD - if there are any)     
    
### Issues Remaining in the program (04/14/2023)
    
      As far as I am aware, there are no glaring* issues in my code. The sending, receiving, forwarding, and moving functionality seems to operate without fault. I am quite excited by this, as it seems with the optimization we did in this lab, I can no longer cause the multiple edge case errors I was experiencing in the previous lab. Buffer overflow cases seem to also have been handled, as after inputting multiple lines of random text, the programs all remain functional (though they don't return any sort of message to inform the user that they're outputting garbage).
      
      *[All that said, I have been able to segment fault my program if multiple of the same kvp are sent (i.e. msg:"1" msg:"2" toPort...). Even in this this case though, only the drone that first receives it will crash (i.e if the sender is close enough for the recipient to directly accept it, or if the recipient tries to forward the message). This is still obviously quite a large issue that needs to be addressed, but ultimately it shouldn't cause a problem for any normal buffers being sent.]