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
Yes.

Yes, those are very satisfactory questions.

Perhaps you may learn, in time.

For now, I have a question for you.

Do you feel fully...satisifed with your powers?

    *[No]
    -> ChoiceTime1
    *[Yes]
        Are you sure?
            ** [Yes]
            -> NoAbilities
            ** [No]
            -> ChoiceTime1
            
=== NoAbilities ===
A most unusual response. 

99.99% of subjects request enhancement.

Unfortunately we cannot proceed without enhancement.

The protocol requires it.
-> ChoiceTime1

=== ChoiceTime1 ===
Please select your enhancement.

    *[Let me leave this unforgiving ground]
    -> SelectJetpack
    *[Let me strike my enemies down]
    -> SelectAttack
    
=== SelectJetpack ===
Intriguing, subject displays an evasive nature. 

This will be a highly elucidating harvest.
# jetpack_ability


We have equipped you with thrusters. Go on, give them a spin. 
# unfreeze_player
# wait_for_jetpack

You feel powerful now, don't you.
-> DONE


=== SelectAttack ===
Intriguing, subject displays an aggressive nature.
#attack_ability
#wait_for_attack

We have transmitted an attack function. Try it out.

You feel powerful now don't you. Please proceed to the exit.
-> END




