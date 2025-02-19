-> Main

=== Main ===
#npc2

Were you afraid of me?

    * [Yes] -> Why
    * [No] -> WhyNot
    *[Can you let me through please?]
    #block_raise
    -> LetMeThrough
    
   
=== Why ===

Why?

    * [Because you're heavy]
    -> SorryImHeavy
    * [I don't know]
        It's ok not to know.
        -> END
    
=== WhyNot === 

Why not?

    * [I don't know]
        It's ok not to know.
        -> END
    * [Because this is just a game]
        -> END
        
=== SorryImHeavy ===

I'm sorry I'm heavy.

-> END

== LetMeThrough ===
Sure.


-> END
    
    
  