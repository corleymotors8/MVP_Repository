-> Main

=== Main ===
#npc1

Were you afraid of me?

    * [Yes] -> Why
    * [No] -> WhyNot
    
   
=== Why ===

Why?

    * [Because you chased me]
    -> LastScene
    * [I don't know]
        It's ok not to know.
    -> END
    
=== WhyNot === 

Why not?

    * [I don't know]
    It's ok not to know.
    -> END
    * [Because this is just a game]
    -> LastScene

== LastScene ==
Ah, I see.

   
-> END