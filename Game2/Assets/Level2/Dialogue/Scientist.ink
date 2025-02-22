-> Main

=== Main ===
I'm sorry we had to destroy you.


    *[How many times has this happened?]
        -> NotTime
    *[Why am I here?]
        -> NotTime
    *[What am I?]
        -> NotTime

=== NotTime ===
...

Perhaps you may learn, in time. 

For now, I have a question for you. 

Are you powerful?

    *[No]
    -> ChoiceTime1
    *[Yes]
    -> ChoiceTime1


=== ChoiceTime1 ===

DEBUG LINE OF DIALOGUE TO CHECK IF LENGTH IS ISSUE

No you are not. 

But we will enhance you. 

Now, select your enhancement.

    *[Let me leave this unforgiving ground]
    -> SelectJetpack
    *[Let me strike my enemies down]
    -> SelectAttack
    
=== SelectJetpack ==
...

Intriguing, subject displays an evasive nature. 

This will be a highly elucidating harvest.
# jetpack_ability
# unfreeze_player
# wait_for_jetpack

We have equipped you with thrusters. Try them with your "spacebar". 


You feel powerful now, don't you. Proceed to the exit.
-> DONE


=== SelectAttack ===
...

Intriguing, subject displays an aggressive nature.

#wait_for_attack
#attack_ability
#unfreeze_player

We have transmitted an attack function. Utilize it with your "b" key.
 
You feel powerful now don't you. Proceed to the exit.

-> END




