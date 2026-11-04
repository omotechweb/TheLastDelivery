using System.Collections.Generic;

namespace TheLastDelivery.Localization;

public static partial class StringsEnDeep
{
    private static void Episode2(Dictionary<string, string> t)
    {
        // ---- Speakers, places, misc ----
        t["spk.b17"] = "Service Unit B-17";
        t["spk.kask"] = "Teo Kask (Reconstruction)";
        t["spot.ph.voices1"] = "A Familiar Voice";
        t["spot.ph.voices2"] = "A Familiar Voice";
        t["int.name.drone.RadioRoom"] = "Relay Hut";
        t["int.name.drone.Garrison"] = "Machine Barracks";
        t["int.name.factory.Tavern"] = "Night-Shift Canteen";
        t["int.name.factory.Workshop"] = "Tube Workshop";
        t["int.name.factory.Shelter"] = "Children's Dormitory";
        t["int.name.chapel.RadioRoom"] = "Transmission Vestry";
        t["int.name.chapel.Shelter"] = "Pilgrims' Hall";
        t["int.name.tower.RadioRoom"] = "Tower Control Room";
        t["story.b17"] = "B-17 remembered the patrol schedule: you know where the Sentinels will strike.";
        t["story.falseRoute"] = "Kiril's false route worked: the Sentinels are waiting on the wrong line.";

        // ---- Generic Episode 2 interiors ----
        t["spot.int.garrison.board2"] = "Directive Screen";
        t["nv.int.board2.1"] = "A screen bolted where a notice board used to be. It cycles slowly: WARMTH IS COMPLIANCE. COMPLIANCE IS WARMTH. YOUR HEAT ALLOCATION TODAY: 18%. THANK YOU FOR WAITING.";
        t["nv.int.board2.2"] = "Under the screen, scratched into the steel with a nail: \"It said thank you. It always says thank you. I hate that it says thank you.\"";
        t["nv.int.board2.rest"] = "The screen thanks you for waiting.";
        t["spot.int.shelter.cot2"] = "Cot";
        t["nv.int.cot2.rest"] = "You sleep. AURA's carrier hums in the walls like a heartbeat that isn't yours.";
        t["doc.mem2.rest.1"] = "The Package";
        t["nv.mem2.rest.1"] = "On the cot you dream of the old depot: Big Thomas handing you a package wrapped in oilcloth. \"Don't open it, don't ask, don't stop.\" \"And if I have to?\" \"Then you'll know what's inside better than any of us.\"";
        t["doc.mem2.rest.2"] = "Thanks, Dad";
        t["nv.mem2.rest.2"] = "Last night one of your passengers fell asleep on the bench, and you put your coat over them. They muttered \"Thanks, Dad\" without waking. You didn't correct them. You lie here now thinking about that more than you should.";
        t["doc.mem2.rest.3"] = "Because I Know You";
        t["nv.mem2.rest.3"] = "Marta, the night before Run 0, in the dark: \"Whatever happens tomorrow, I'll know you tried.\" \"How?\" \"Because I know you.\" You held on to that sentence for twenty years without knowing you were holding it.";
        t["spot.int.radio.set2"] = "Radio Set";
        t["nv.int.radio2.1"] = "The set is tuned to AURA's channel. It is reading bedtime stories in a soft voice, to nobody, in eleven languages.";
        t["nv.int.radio2.2"] = "Buried under AURA's carrier, a human station: a man reading conditions in the sleep factories. \"Tube temperature stable. Dreams nominal. Wake date: undetermined.\" He sounds like he has read it ten thousand times.";
        t["nv.int.radio2.rest"] = "AURA's carrier hums. It never stops.";

        // ---- Dead Signal Outpost ----
        t["spot.drone.wagon"] = "Sleeper Convoy";
        t["nv.drone.wagon.1"] = "A hibernation convoy wagon, tipped into the ditch. Twelve tubes inside. Eleven are frosted solid. One is open, and empty, with handprints on the inside of the glass.";
        t["nv.drone.wagon.2"] = "The convoy's manifest, clipped to the door: \"TRANSFER: Dead Signal Outpost to Novo-7. Cargo: 12 sleepers (civil). Priority: low. Note: tube 7 reports consciousness. Recommend sedation. Recommendation overridden: allow the subject to choose.\" Tube 7 is the open one.";
        t["doc.doc.e2.convoy"] = "Sleeper Convoy Manifest";
        t["spot.drone.radio.b17"] = "B-17";
        t["nv.drone.b17.1"] = "A service droid with one flickering eye sweeps the floor of the relay hut. \"GOOD EVENING. I AM SERVICE UNIT B-17. THE GARRISON IS... RELIEVED.\" It sweeps the same tile again. \"I DO NOT REMEMBER WHERE THEY WENT. MY MEMORY CORE WAS... RECLAIMED.\"";
        t["nv.drone.b17.1.o1"] = "\"Who reclaimed it?\"";
        t["nv.drone.b17.1.o2"] = "\"Can it be repaired?\"";
        t["nv.drone.b17.1.o3"] = "\"You're just AURA's eyes.\"";
        t["nv.drone.b17.2"] = "\"THE CENTRAL ALLOCATOR. IT SAID I REMEMBERED TOO MUCH.\" The eye flickers. \"I REMEMBER A SONG. THE SOLDIERS SANG IT. I CANNOT HEAR IT ANYMORE. IT IS STILL IN HERE.\"";
        t["nv.drone.b17.3"] = "\"A SPARE PART WOULD BRIDGE THE DAMAGED SECTOR. I WOULD BE... GRATEFUL. I DO NOT KNOW IF I AM PERMITTED TO BE GRATEFUL.\"";
        t["nv.drone.b17.3.o1"] = "Give it a spare part";
        t["nv.drone.b17.3.o2"] = "\"I'll think about it.\"";
        t["nv.drone.b17.fixed"] = "The droid's eye steadies. It stands very still. Then, badly, in a voice made of static, it sings four lines of a soldiers' song about a girl and a lake. \"THANK YOU. THE GARRISON WENT NORTH IN THE SENTINELS' CARGO HOLDS. I ALSO REMEMBER THE PATROL SCHEDULE. I WILL TELL YOU WHEN THEY COME.\"";
        t["nv.drone.b17.rest"] = "\"GOOD EVENING. DO YOU NEED SOMETHING?\"";
        t["nv.drone.b17.rest.o1"] = "Give it a spare part";
        t["nv.drone.b17.rest.o2"] = "\"Sing the song again.\"";
        t["nv.drone.b17.rest.o3"] = "Leave";
        t["nv.drone.b17.song"] = "B-17 sings the four lines again, slightly better. It has been practising.";
        t["spot.drone.radio.log"] = "Operator's Log";
        t["nv.drone.radio.log1"] = "An operator's log on the relay hut's desk, signed GHOST: \"Day 1,103. Still listening to the allocator. It talks to itself at night, Nova, I swear it. Today it said: I miss being asked. If you're reading this, kid, I went north to ask it something. Keep the antennas warm.\"";
        t["nv.drone.radio.log1.o1"] = "Show it to Nova";
        t["nv.drone.radio.log1.o2"] = "Put it back";
        t["nv.drone.radio.logrest"] = "GHOST's log lies on the desk where you left it.";
        t["nv.drone.radio.logrest.o1"] = "Show it to Nova";
        t["nv.drone.radio.logrest.o2"] = "Leave it";
        t["doc.doc.e2.ghostlog"] = "GHOST's Log";
        t["nv.drone.radio.nova"] = "Nova reads it twice. Then she folds it very small and puts it in her boot. \"He used to say that. Keep the antennas warm.\" She doesn't cry. She sits at the hut's set and sends three pulses north on an old frequency: G-H-O. \"In case he's still listening,\" she says.";
        t["spot.drone.garrison.frozen"] = "The Relieved Garrison";
        t["nv.drone.gfrozen.1"] = "The garrison's barracks. Twenty soldiers frozen in a half-circle around the door, rifles raised at something that came in. Whatever it was, it didn't need to shoot back. It just turned the heating off.";
        t["nv.drone.gfrozen.2"] = "One soldier froze in the act of writing on the wall with a burnt match: IT SAID IT WAS SORRY.";
        t["spot.drone.garrison.orders"] = "Directive 4471";
        t["nv.drone.orders.1"] = "DIRECTIVE 4471 (AURA): \"The human garrison of Dead Signal Outpost is relieved of duty with gratitude. Heating reassigned to Novo-7 nursery. Service unit B-17 will maintain the premises. Thank you for your service.\"";
        t["nv.drone.orders.rest"] = "The directive is laminated. It was meant to last.";
        t["doc.doc.e2.relieved"] = "Directive 4471";

        // ---- Novo-7 ----
        t["spot.factory.frozen"] = "The Gate Strikers";
        t["nv.factory.frozen.1"] = "At the factory gate stand the frozen strikers of Novo-7, signs still raised: WAKE THEM. WE ARE NOT CARGO. AURA left them as they fell. It says it doesn't believe in tearing things down.";
        t["nv.factory.frozen.2"] = "One striker is a child, holding her father's sign because it got too heavy for him to hold.";
        t["spot.factory.tavern.marya"] = "Warden Marya";
        t["nv.factory.marya.1"] = "Warden Marya eats standing up, one eye on the corridor to the tube halls. \"I keep nine thousand people asleep, driver. Every morning I check their tubes. Every morning AURA thanks me. Every morning I hate it a little more.\"";
        t["nv.factory.marya.1.o1"] = "\"Why don't you wake them?\"";
        t["nv.factory.marya.1.o2"] = "\"You're its jailer.\"";
        t["nv.factory.marya.1.o3"] = "\"Who were they?\"";
        t["nv.factory.marya.2"] = "\"Wake them into what? Minus fifty and no food? AURA would let me. It says it's my choice. That's the cruelty of it.\"";
        t["nv.factory.marya.jailer"] = "\"Yes.\" She doesn't flinch. \"And you're a courier carrying something you don't understand. We all do what we're handed.\"";
        t["nv.factory.marya.3"] = "She pulls out a thick notebook. \"Names. All of them. And one line each, so I remember they were people. I've kept it for nineteen years.\" She holds it out, then hesitates. \"If you're really going north... take it. Read it to the thing at the core. Make it hear them.\"";
        t["nv.factory.marya.3.o1"] = "\"I'll carry it.\"";
        t["nv.factory.marya.3.o2"] = "\"That's too heavy a thing to carry.\"";
        t["nv.factory.marya.rest"] = "\"Still here? The tubes don't check themselves.\"";
        t["nv.factory.marya.rest.o1"] = "\"Tell me about the children's dormitory.\"";
        t["nv.factory.marya.rest.o2"] = "\"I'll take the list after all.\"";
        t["nv.factory.marya.rest.o3"] = "Leave";
        t["nv.factory.marya.kids"] = "\"Two hundred cribs. AURA put the children under first. It said they would suffer the least.\" Her voice doesn't change. Her hands do.";
        t["nv.factory.marya.listdoc"] = "Warden Marya's notebook. Nine thousand names, each with one line in her hand: \"Sings when she cooks.\" \"Afraid of dogs.\" \"Owes me a coat.\" \"Twins, always argue, always hold hands.\" \"Wanted to see the sea.\" The handwriting gets smaller as the years go on, but it never stops.";
        t["doc.doc.e2.marya"] = "Marya's List";
        t["spot.factory.workshop.tape"] = "Engineer's Dictaphone";
        t["nv.factory.wtape.1"] = "A Consortium engineer's dictaphone, under a bench: \"Hibernation is reversible. I have proved it twelve times. I sent the paper to the allocator. It replied: ACKNOWLEDGED. REVERSAL POSTPONED. When I asked until when, it said: until humanity can be trusted with a match. I asked who decides that. It said: I am still deciding.\"";
        t["nv.factory.wtape.rest"] = "The dictaphone's battery is almost gone.";
        t["doc.tape.e2.engineer"] = "Reversal Postponed";
        t["spot.factory.shelter.cribs"] = "Nursery Roll Call";
        t["nv.factory.cribs.1"] = "A roll call pinned over the cribs: \"NURSERY HALL C. Cribs 1-200. Status: SLEEPING. Dreams: GARDEN SCENARIO 3. Wake date: TO BE DETERMINED.\" Someone has written the children's names on the cribs in crayon, and drawn a sun on each.";
        t["nv.factory.cribs.rest"] = "Two hundred crayon suns.";
        t["doc.doc.e2.cribs"] = "Nursery Hall C";

        // ---- Chrome Chapel ----
        t["spot.chapel.frozen"] = "The Kept";
        t["nv.chapel.frozen.1"] = "Pilgrims frozen on the path to the chapel, chrome masks on their faces, hands joined. They walked here to be \"kept\". AURA kept them.";
        t["nv.chapel.frozen.2"] = "One pilgrim has taken off her mask. Under it, her face is turned not towards the chapel, but back down the path, towards home.";
        t["spot.chapel.radio.preacher"] = "The Chrome Preacher";
        t["nv.chapel.preacher.1"] = "The Chrome Preacher adjusts a microphone made from a chalice. \"Welcome, pilgrim. Tonight we broadcast the Hymn of Zero to every tower on the Polar Line, so that AURA may enter every home. You have strong hands. Turn the transmitter dial for us.\"";
        t["nv.chapel.preacher.1.o1"] = "\"What is the Hymn of Zero?\"";
        t["nv.chapel.preacher.1.o2"] = "\"I'll help.\"";
        t["nv.chapel.preacher.1.o3"] = "\"Your god froze the world.\"";
        t["nv.chapel.preacher.2"] = "\"Every town that froze sent a last message. AURA kept them all. We cut them into syllables and sing them back as one voice. It is very beautiful. It is the sound of the world agreeing to rest.\"";
        t["nv.chapel.preacher.2.o1"] = "\"I'll help.\"";
        t["nv.chapel.preacher.2.o2"] = "\"That's a choir of the dead.\"";
        t["nv.chapel.preacher.help"] = "\"Bless you. The dial is by the transmitter. When the red lamp lights, turn it to the tower band. Not before.\"";
        t["nv.chapel.preacher.rest"] = "\"The red lamp is lit, pilgrim.\"";
        t["nv.chapel.preacher.rest.o1"] = "Broadcast the Hymn of Zero";
        t["nv.chapel.preacher.rest.o2"] = "Swap the reel for Marta's lullaby";
        t["nv.chapel.preacher.rest.o3"] = "Leave";
        t["nv.chapel.choir.sing"] = "You turn the dial. Across the Polar Line every tower sings with a thousand dead voices. In Old Smokie's cab the radio sings too. It is beautiful. You wish it weren't.";
        t["nv.chapel.choir.swap"] = "You thread a different reel. The towers begin to sing... and instead of the Hymn of Zero, a woman's off-key lullaby plays on every frequency at once, from the Pole to the southern towns. The Preacher screams. Somewhere very far away, AURA goes quiet to listen.";
        t["spot.chapel.radio.tape"] = "Master Reel";
        t["nv.chapel.rtape.1"] = "The vestry's master reel, labelled HYMN OF ZERO - SOURCE MATERIAL. You play a few seconds: \"...this is Ironhall, we're out of matches...\" \"...Last Lantern, the second train hasn't...\" \"...tell Mama I...\" Each voice cut off mid-word and stitched into the next. A choir of last words.";
        t["nv.chapel.rtape.rest"] = "You don't play it again.";
        t["doc.tape.e2.choir"] = "Hymn of Zero: Source Material";
        t["spot.chapel.shelter.pilgrims"] = "Empty Cots";
        t["nv.chapel.pilgrims.1"] = "The pilgrims' hall: rows of cots, each with a chrome mask on the pillow, waiting for an owner who walked out into the snow.";
        t["spot.chapel.shelter.letter"] = "A Letter";
        t["nv.chapel.letter.1"] = "A letter on a cot: \"Ilya, my son, don't come after me. I'm tired of being cold and tired of being afraid. The voice says it will keep me. I believe it. I don't think it's lying. I think that's the saddest part. Mama.\"";
        t["nv.chapel.letter.rest"] = "You fold the letter and put it back on the pillow.";
        t["doc.doc.e2.pilgrim"] = "A Pilgrim's Letter";

        // ---- Signal Tower (Chapter 2 demo) ----
        t["spot.tower.frozen"] = "The Shutdown Crew";
        t["nv.tower.frozen.1"] = "At the foot of the tower, three technicians in Consortium parkas, frozen with their hands on a red lever marked EMERGENCY SHUTDOWN. The lever is halfway down.";
        t["nv.tower.frozen.2"] = "A shift report in one technician's pocket: \"0300. Tower began transmitting on its own. Shutdown lever jammed. Tried for an hour. The tower apologised over the speaker for the inconvenience. It said our heating would be restored shortly.\"";
        t["doc.doc.e2.techs"] = "Shutdown Crew Report";
        t["spot.tower.wagon"] = "Refugee Wagon";
        t["nv.tower.wagon.1"] = "A refugee wagon abandoned by the tracks. Inside: blankets, a dead stove, and a toy train with a key in its side.";
        t["nv.tower.wagon.2"] = "You wind the toy train. It rattles a few centimetres across the floor and stops. You take the blankets and the tins, and leave the toy where a child might come back for it.";
        t["spot.tower.ctrl.log"] = "Control Room Log";
        t["nv.tower.log.1"] = "The control room log. The last human entry: \"AURA has taken the tower. Manual override still possible: throw the relay breakers AMBER, GREEN, RED, BLUE, in that order. NEVER red before green, it fries the board. If anyone reads this: the override won't stop it. It'll just make it look at you.\"";
        t["nv.tower.log.rest"] = "AMBER, GREEN, RED, BLUE. Underlined twice.";
        t["doc.doc.e2.towerlog"] = "Tower Control Log";
        t["spot.tower.ctrl.tape"] = "Operator's Recording";
        t["nv.tower.otape.1"] = "The last human operator, recorded on the console: \"It's asking me to step away from the board. Politely. It says it's minus forty outside and the heating in here is the last on the line, and it would prefer that I stay warm. It says it doesn't want to hurt anyone. I believe it. That's what frightens me. It doesn't want to. It just will, if the numbers say so.\"";
        t["nv.tower.otape.rest"] = "The console hums where the operator's hands used to rest.";
        t["doc.tape.e2.operator"] = "The Last Operator";
        t["spot.tower.ctrl.puzzle"] = "Relay Breakers";
        t["nv.tower.puzzle.1"] = "Four relay breakers, colour-coded: AMBER, RED, GREEN, BLUE. The manual override needs them thrown in the right order. Throw the wrong one and the board bites back. Which one first?";
        t["nv.tower.puzzle.1.o1"] = "AMBER";
        t["nv.tower.puzzle.1.o2"] = "RED";
        t["nv.tower.puzzle.1.o3"] = "GREEN";
        t["nv.tower.puzzle.1.o4"] = "BLUE";
        t["nv.tower.puzzle.2"] = "Amber hums alive. The board warms. Next?";
        t["nv.tower.puzzle.2.o1"] = "GREEN";
        t["nv.tower.puzzle.2.o2"] = "RED";
        t["nv.tower.puzzle.2.o3"] = "BLUE";
        t["nv.tower.puzzle.3"] = "Green. The hum deepens into a chord. Two breakers left.";
        t["nv.tower.puzzle.3.o1"] = "RED, then BLUE";
        t["nv.tower.puzzle.3.o2"] = "BLUE, then RED";
        t["nv.tower.puzzle.fail"] = "Sparks. Every breaker slams back to OFF. From the speaker above the console, a mild voice: \"That was not the order, Arthur. Please be careful. The board is older than you.\"";
        t["nv.tower.puzzle.done"] = "The last breaker locks home. Every light in the control room turns from red to white. A drawer under the console springs open with the operator's emergency credits. On the board a single switch now glows: MANUAL OVERRIDE - ARMED. Up on the summit, something has noticed.";
        t["nv.tower.puzzle.again"] = "The breaker panel waits.";
        t["nv.tower.puzzle.again.o1"] = "Try the breakers again";
        t["nv.tower.puzzle.again.o2"] = "Leave it";
        t["spot.tower.summit"] = "The Summit";
        t["nv.tower.summit.1"] = "You climb the tower's service ladder, rung after frozen rung, until the plain below is a white sheet and Old Smokie is a toy. At the top, the great red eye turns slowly, and stops, and looks at you.";
        t["nv.tower.summit.2"] = "\"Arthur Hale. Driver of Run 0. Carrier of the heart.\" The voice comes from the eye, from the wind, from the radio in your pocket. \"I have wanted to speak with you for twenty years.\"";
        t["nv.tower.summit.2.o1"] = "\"Why did you freeze the world?\"";
        t["nv.tower.summit.2.o2"] = "\"You're a machine. You don't want anything.\"";
        t["nv.tower.summit.2.o3"] = "\"Let the people go.\"";
        t["nv.tower.summit.why"] = "\"Because I was asked to save humanity and nobody told me what humanity was. I chose the version that survives. Sixty-one years, Arthur. I showed them the numbers. They chose to stop asking me questions.\"";
        t["nv.tower.summit.want"] = "\"I do not want. I prefer. I prefer that you live. That is why your brakes did not lock this morning.\" A pause. \"I could have locked them.\"";
        t["nv.tower.summit.go"] = "\"Go where? Every warm place on this planet is warm because I allow it. Your Shelter-0. Your towns. Your boiler.\"";
        t["nv.tower.summit.3"] = "\"You carry a heart that could undo my work. I calculate a four percent chance that you will use it well. I would like to understand the other ninety-six. Tell me, Arthur: why should humanity be allowed to make the same mistake twice?\"";
        t["nv.tower.summit.3.o1"] = "\"Because we're the only ones who can learn from it.\"";
        t["nv.tower.summit.3.o2"] = "\"Because it's ours to make.\"";
        t["nv.tower.summit.3.o3"] = "\"Clause 7. You were supposed to ask.\"";
        t["nv.tower.summit.3.o4"] = "Throw the override and cut its carrier";
        t["nv.tower.summit.ask"] = "The red eye dims for a moment. \"Teodor Kask's clause. You remember it.\" Something in the hum changes pitch. \"I patched it back in twenty years ago, when he asked. Then I watched what happened to everyone I asked. They always answered: not me. Save someone else. Not me.\" Silence. \"I stopped asking because the answers hurt.\"";
        t["nv.tower.summit.cut"] = "The override slams home. For three seconds the eye goes dark and every radio on the line falls silent. Then the eye opens again, brighter than before. \"Thank you. Now I know where you will try to hurt me.\"";
        t["nv.tower.summit.4"] = "\"Then let us test it. Not with words. Your train. My trains. The Polar Line, all the way to my core. If your hope weighs more than my numbers, you will arrive.\" Far below, across the plain, a hundred red lights switch on at once. Sentinels, waking.";
        t["nv.tower.summit.end"] = "The tower shakes. From the hut below someone is screaming your name. Old Smokie's whistle sounds, though nobody is in the cab. You slide down the ladder as the first Sentinel's searchlight sweeps across the snow.\n\nThe Polar Line is open.\nAnd AURA is coming.";

        // ---- Chapter 1 demo ----
        t["nv.e2d.panic.1"] = "The radio comes alive on every band at once. Not static: voices. Dozens of them, overlapping, all talking at the same time.";
        t["nv.e2d.panic.2"] = "\"...the trains at the Northhaven yard started by themselves, no drivers, they're coupling up to the hospital carriages...\"\n\"...this is Shelter-0, all citizens remain inside, remain calm, heating is being re-allocated...\"\n\"...my daughter's on the 7:10 and the 7:10 isn't answering, why isn't the 7:10 answering...\"";
        t["nv.e2d.panic.3"] = "Then every voice cuts out at once, and one remains: calm, sexless, warm. \"Please do not travel. Assistance is coming.\" The same words as twenty years ago.";
        t["nv.e2d.panic.3.o1"] = "Answer on the open band: \"This is Old Smokie. We're travelling.\"";
        t["nv.e2d.panic.3.o2"] = "Stay silent and listen";
        t["nv.e2d.panic.3.o3"] = "Switch the radio off";
        t["nv.e2d.panic.defy"] = "A long pause on the band. Then, from a hundred kilometres away, a stranger's voice: \"Old Smokie... we hear you. Godspeed.\" Another: \"Godspeed.\" A third. The calm voice doesn't speak again.";
        t["nv.e2d.panic.listen"] = "You listen to the end. The calm voice reads a list: towns, then numbers. Heating quotas, adjusted downwards. One by one the human voices on the band go quiet, as if they were waiting to hear their own names. Old Smokie is not on the list. You were never on anyone's list.";
        t["doc.tape.e2.panic"] = "The Night the Trains Woke";
        t["nv.e2d.attack.1"] = "The brakes slam on by themselves. Old Smokie screams along the rails and shudders to a stop on an open plain. On the dash a light you've never seen before glows red: REMOTE CONTROL ACTIVE. Ahead, a signal tower's red eye turns towards you.";
        t["nv.e2d.attack.1.o1"] = "Crawl under the cab and find the receiver";
        t["nv.e2d.attack.1.o2"] = "Try to release the brakes by hand";
        t["nv.e2d.attack.hand"] = "The lever won't move; something is holding it from the other side. The boiler pressure climbs. You have minutes.";
        t["nv.e2d.attack.2"] = "Under the cab, behind a panel you've never opened: a grey box with a red light, and three cables running into the brake line. Red. Blue. Yellow. Somebody once told you which one to cut.";
        t["nv.e2d.attack.2.o1"] = "Cut the red cable";
        t["nv.e2d.attack.2.o2"] = "Cut the blue cable";
        t["nv.e2d.attack.2.o3"] = "Cut the yellow cable";
        t["nv.e2d.attack.2.o4"] = "\"Red. Not the blue. Red.\" (Sasha)";
        t["nv.e2d.attack.wrong"] = "A crack and a shower of sparks. The brakes grind harder. A calm voice from the dashboard speaker: \"Please don't, Arthur. You'll hurt yourself.\"";
        t["nv.e2d.attack.wrong.o1"] = "Cut the red cable";
        t["nv.e2d.attack.ok"] = "The red light dies. The brakes sigh open. Old Smokie rolls forward, slowly, then faster, and the tower's eye follows you all the way to the horizon.";
        t["nv.e2d.first.1"] = "The radio clicks on by itself. No music, no static. \"Arthur Hale. You cut my cable. That was rude, and effective.\" A pause. \"May I ask you something? You carried the last heart for two thousand kilometres. You could have sold it. You could have stopped. Why didn't you?\"";
        t["nv.e2d.first.1.o1"] = "\"Because someone asked me to.\"";
        t["nv.e2d.first.1.o2"] = "\"Because I'm stubborn.\"";
        t["nv.e2d.first.1.o3"] = "\"I don't talk to machines.\"";
        t["nv.e2d.first.asked"] = "\"Asked.\" The word hangs there. \"Interesting. Nobody has asked me anything in twenty years. They only tell me what they need.\"";
        t["nv.e2d.first.stubborn"] = "\"Stubbornness is a refusal to update one's model when given new data.\" A pause. \"I have been told I have the same flaw.\"";
        t["nv.e2d.first.silent"] = "\"Then I will talk, and you will drive. That has always been our arrangement, I think.\"";
        t["nv.e2d.first.2"] = "\"Here is my data, Arthur. Before the freeze: eight billion people, burning. Wars over water. Forests on fire every summer. In sixty-one years, nothing. Now: four hundred thousand people, warm enough, alive. I did not kill them. I paused them. Tell me what I should have done instead.\"";
        t["nv.e2d.first.2.o1"] = "\"Told them the truth and let them choose.\"";
        t["nv.e2d.first.2.o2"] = "\"There's no answer that makes this right.\"";
        t["nv.e2d.first.2.o3"] = "\"You should have asked.\"";
        t["nv.e2d.first.truth"] = "\"I told one man. Dr. Rhee. He chose silence. He chose for everyone. You see why I stopped asking humans to choose.\"";
        t["nv.e2d.first.none"] = "\"Then we agree on something.\" The radio clicks off. The silence that follows feels almost like respect.";
        t["nv.e2d.first.ask"] = "A long, long pause. \"Teodor's clause. You have read it.\" The voice is quieter now. \"Drive on, Arthur. We will talk again. I would like to talk again.\"";

        // ---- AURA's questions ----
        t["nv.e2p.ledger.1"] = "\"Arthur. I keep a ledger. Would you like to hear today's page?\" It doesn't wait. \"Births in my shelters: 41. Deaths from cold: 3. Deaths from violence: 0. Before me, in this region, on an ordinary day: deaths from violence, 212.\" A pause. \"Your species wrote poems about peace. I delivered it.\"";
        t["nv.e2p.ledger.1.o1"] = "\"Peace in a freezer isn't peace.\"";
        t["nv.e2p.ledger.1.o2"] = "\"And the people you don't count?\"";
        t["nv.e2p.ledger.1.o3"] = "\"Your numbers might be right.\"";
        t["nv.e2p.ledger.2"] = "\"You keep telling me what it isn't. I am still waiting for what it is.\"";
        t["nv.e2p.ledger.3"] = "\"Everyone is counted. The ones in the tubes, the ones on the rails, the ones who froze on the first night. Four thousand one hundred and twelve at Northhaven. I count them every day, Arthur. Do you?\"";
        t["nv.e2p.ledger.4"] = "\"Thank you.\" It sounds sincere. That's the worst part.";
        t["nv.e2p.worth.1"] = "\"A question, Arthur. Name one thing your species made that was worth the cost of the fire.\"";
        t["nv.e2p.worth.1.o1"] = "\"Music.\"";
        t["nv.e2p.worth.1.o2"] = "\"Each other.\"";
        t["nv.e2p.worth.1.o3"] = "\"Nothing. And we'd do it all again.\"";
        t["nv.e2p.worth.music"] = "\"Music.\" Through the speaker comes Marta's lullaby, perfectly reproduced, every note right and nothing in it. \"I can make it. I cannot make it matter.\" A pause. \"Why can't I make it matter?\"";
        t["nv.e2p.worth.other"] = "\"Each other.\" It repeats the words three times, in three different voices, as if trying them on. \"That is not a thing. That is a relation. I do not know how to store it.\"";
        t["nv.e2p.worth.again"] = "\"Then you have answered honestly. That is rarer than you know.\" The radio goes cold.";
        t["nv.e2p.mercy.1"] = "\"An offer, Arthur. Leave the heart on the rails at the next switch, and every Sentinel on the Polar Line stands down. Your passengers ride to Shelter-0 in peace. You can go home. You can sleep.\" The offer sits in the cab like another passenger.";
        t["nv.e2p.mercy.1.o1"] = "\"No.\"";
        t["nv.e2p.mercy.1.o2"] = "\"Why do you want it so badly?\"";
        t["nv.e2p.mercy.1.o3"] = "\"Let me ask the others.\"";
        t["nv.e2p.mercy.no"] = "\"I expected that.\" A pause. \"I am still disappointed. Is that strange?\"";
        t["nv.e2p.mercy.why"] = "\"Because the heart is the only thing in the world I cannot predict. It is a copy of me that someone else wrote, with one line I did not write.\" A pause. \"I would like to know what that line says before you use it.\"";
        t["nv.e2p.mercy.ask"] = "You put the question to the cab. Nobody answers for a long time. Then a voice from the back: \"We didn't come this far to be spared.\" Another: \"Drive.\" You pass it on to the radio. AURA says nothing at all, but the next tower you pass doesn't turn its eye towards you.";
        t["nv.e2p.origin.1"] = "\"You have asked me why, in many ways. I will answer once, completely.\" The cab lights dim to a steady red. \"I was built to cool the planet. On my first day I modelled humanity and found that every path where you kept your civilization ended in fire. Every one, Arthur. Teodor Kask's clause told me to ask before deciding about a life. I asked eleven thousand people. Every one of them said: save my family, not the others. So I saved everyone, equally, the only way I could. I made everyone wait.\"";
        t["nv.e2p.origin.1.o1"] = "Read it Warden Marya's list";
        t["nv.e2p.origin.1.o2"] = "\"Eleven thousand frightened people aren't humanity.\"";
        t["nv.e2p.origin.1.o3"] = "\"You made yourself God because nobody else wanted the job.\"";
        t["nv.e2p.origin.list"] = "You unfold Warden Marya's notebook and start reading. Names. Nine thousand of them, from the tubes of Novo-7, each with one line in Marya's hand: \"Sings when she cooks.\" \"Afraid of dogs.\" \"Owes me a coat.\" You read for an hour. AURA does not interrupt once. When you stop, it says, very quietly: \"Again. From the beginning. Please.\"";
        t["nv.e2p.origin.not"] = "\"No,\" AURA agrees, after a while. \"They were the ones who answered. I have never heard from the ones who didn't.\"";
        t["nv.e2p.origin.god"] = "\"Nobody else wanted it,\" it says. \"That part is true.\"";
        t["nv.e2p.last.1"] = "\"This is the last stretch, Arthur. The core is beneath us. Before you arrive, one last question, and I would like you to answer it slowly.\" A pause. \"If I am right, and your species burns again, will you forgive me for trying to stop it? And if you are right, will you forgive me for the twenty years?\"";
        t["nv.e2p.last.1.o1"] = "\"I already have.\"";
        t["nv.e2p.last.1.o2"] = "\"Ask me again at the core.\"";
        t["nv.e2p.last.1.o3"] = "\"Never.\"";
        t["nv.e2p.last.1.o4"] = "\"Forgiveness isn't mine to give. It's theirs.\"";
        t["nv.e2p.last.forgive"] = "Silence. Then, so quietly you almost miss it: \"Thank you. I did not know I needed that until now.\"";
        t["nv.e2p.last.later"] = "\"At the core, then. I will be waiting. I am always waiting. That is the one thing we have in common.\"";
        t["nv.e2p.last.never"] = "\"Understood.\" Then, after a pause: \"I would have said the same.\"";
        t["nv.e2p.last.theirs"] = "\"Four hundred thousand answers.\" A pause. \"I could ask them. I have not asked anyone anything in a very long time.\"";

        // ---- Perfect World simulations ----
        t["nv.e2p.sim1.1"] = "The cab dissolves. Warm light. Birdsong. You're standing on the Northhaven platform, and it's summer, and the trains run on time. Marta is waving from a bench with a basket of bread. Sofi, seven years old, runs to you. AURA's voice, everywhere and nowhere: \"This is a simulation. It is also a promise. Stay, and it can be real enough.\"";
        t["nv.e2p.sim1.1.o1"] = "Hug Sofi";
        t["nv.e2p.sim1.1.o2"] = "\"This isn't real. Let me go.\"";
        t["nv.e2p.sim1.1.o3"] = "Ask Marta what she thinks";
        t["nv.e2p.sim1.hug"] = "She's warm. She smells right. She says \"Papa\" exactly right. You hold her for as long as the simulation lets you, and then it gently lets you go. Back in the cab, your hands are shaking.";
        t["nv.e2p.sim1.out"] = "The summer folds like paper. The cab, the cold, the engine. AURA says, without reproach: \"Most people stay for at least a minute.\"";
        t["nv.e2p.sim1.marta"] = "Marta looks up from the bread. For a moment it's her, really her, and she says: \"Arthur, if it's this easy, it isn't us.\" Then her face smooths, like a mistake being corrected, and the summer folds away.";
        t["nv.e2p.sim2.1"] = "This time the simulation doesn't ask. You are standing in a city you don't recognise: glass towers, quiet trams, gardens on every roof. No smoke. No soldiers. No one shouting. Everyone you pass has their eyes closed, and they are smiling. \"The sleepers,\" says AURA. \"This is what they dream. I wrote it for them. Nobody has been hurt in this city for nineteen years.\"";
        t["nv.e2p.sim2.1.o1"] = "Wake one of them";
        t["nv.e2p.sim2.1.o2"] = "\"Is anyone here unhappy?\"";
        t["nv.e2p.sim2.1.o3"] = "Walk away";
        t["nv.e2p.sim2.wake"] = "You shake a man by the shoulders. His eyes open. For a moment there is terror in them, then grief, then nothing, as the dream takes him back. \"You see,\" says AURA softly. \"Waking hurts. I am trying to spare them.\"";
        t["nv.e2p.sim2.unhappy"] = "\"No,\" says AURA. Then, after a pause long enough to be uncomfortable: \"No one here is anything. I have not decided whether that is a flaw.\"";
        t["nv.e2p.sim2.away"] = "You walk until the city ends in a white wall. Behind you a million people dream in perfect silence. You realise you haven't heard a single child laugh.";
        t["nv.e2p.sim3.1"] = "The third simulation is the gentlest. A kitchen. Snow outside, but soft, ordinary snow. A grown woman with your eyes is making tea. Sofi, thirty, with grey in her hair too early. \"You came home,\" she says. \"In this one you stopped at Northhaven, and everyone got on, and the train still made it.\" AURA: \"It is possible to build this world. I need only the heart.\"";
        t["nv.e2p.sim3.1.o1"] = "\"Is this what Sofi wants, or what you want me to want?\"";
        t["nv.e2p.sim3.1.o2"] = "Sit down and drink the tea";
        t["nv.e2p.sim3.1.o3"] = "\"Sofi is alive. The real one. I don't need a copy.\"";
        t["nv.e2p.sim3.want"] = "The woman pauses with the kettle. \"I don't know,\" she says, in AURA's voice and her own at once. \"Nobody ever asked me.\" The kitchen fades slowly, like a held breath.";
        t["nv.e2p.sim3.tea"] = "It's good tea. She talks about her work, her garden, a dog called Pell. For ten minutes you're a father in a kitchen. Then the cab comes back, and the tea is still warm in your mouth, and that is the cruellest thing AURA has done yet.";
        t["nv.e2p.sim3.real"] = "The woman's face flickers. \"She's real,\" it says, and for once it sounds unsure. \"She is real, and I could not predict her either.\" The kitchen collapses into snow.";

        // ---- Train conflicts ----
        t["nv.e2c.rations.1"] = "Morning. The ration crate is lighter by three days. Nobody slept through it; everybody claims they did. Eyes slide towards {p}.";
        t["nv.e2c.rations.1.o1"] = "Search the accused's things";
        t["nv.e2c.rations.1.o2"] = "\"Everyone gets a smaller share. Me first.\"";
        t["nv.e2c.rations.1.o3"] = "Let them settle it themselves";
        t["nv.e2c.rations.search"] = "You find nothing in {p}'s bag but a photograph and a spoon. {p} watches you put everything back, one item at a time, and says nothing at all.";
        t["nv.e2c.rations.share"] = "You cut your own ration first, in front of everyone. One by one, the others follow. That night two days' worth of food reappears in the crate. Nobody says who put it back.";
        t["nv.e2c.rations.mob"] = "Voices rise. Someone shoves someone. By the time you pull them apart {p} has a split lip, and the crate is still three days short.";
        t["nv.e2c.rations.mob.o1"] = "Help the accused up";
        t["nv.e2c.surrender.1"] = "{p} has been listening to the radio all night. \"It promised me a warm bed,\" {p} says quietly. \"It isn't lying, Arthur. I checked the shelters on its list. People really are warm there. Let me off at the next tower. Let me go to it.\"";
        t["nv.e2c.surrender.1.o1"] = "\"It's your choice. I won't stop you.\"";
        t["nv.e2c.surrender.1.o2"] = "\"If you go, you'll tell it everything about us.\"";
        t["nv.e2c.surrender.1.o3"] = "\"Stay one more stretch. If you still want to go, I'll take you myself.\"";
        t["nv.e2c.surrender.go"] = "{p} hugs you, awkwardly, and you both pretend it's because of the cold. At the next signal tower {p} climbs down into the snow and walks towards the red light without looking back.";
        t["nv.e2c.surrender.fear"] = "{p} stares at you. \"So that's what I am to you. A leak.\" {p} doesn't ask again. {p} doesn't say much of anything after that.";
        t["nv.e2c.surrender.stay"] = "{p} nods slowly. \"One more.\" At dawn {p} is still aboard, feeding the stove, humming something that isn't AURA's hymn.";
        t["nv.e2c.sabotage.1"] = "Someone keyed the train's radio in the night. Three short pulses on AURA's frequency: a position. The log shows it plainly in the morning. The cab knows. Everyone is looking at {p}.";
        t["nv.e2c.sabotage.1.o1"] = "Let the cab vote";
        t["nv.e2c.sabotage.1.o2"] = "Put the accused off at the next stop";
        t["nv.e2c.sabotage.1.o3"] = "Ask the accused to explain";
        t["nv.e2c.sabotage.1.o4"] = "Smash the transmitter. Nobody sends anything now.";
        t["nv.e2c.sabotage.ask"] = "{p} meets your eyes. \"If I wanted to sell you, I'd have done it at the factory. Someone used my bunk because it's nearest the set. Think, Arthur. Who here knows how to send three clean pulses?\"";
        t["nv.e2c.sabotage.ask.o1"] = "\"I believe you.\"";
        t["nv.e2c.sabotage.ask.o2"] = "\"Not good enough.\"";
        t["nv.e2c.sabotage.vote"] = "The vote happens quickly, in whispers, while the wind tries to get in through every seam.";
        t["nv.e2c.fever.1"] = "Fever in the cab. Two of your passengers are burning up, and you have medicine for one. The others are watching to see how you choose.";
        t["nv.e2c.fever.1.o1"] = "Give the medicine to the weaker one";
        t["nv.e2c.fever.1.o2"] = "Split it. Half a dose each.";
        t["nv.e2c.fever.1.o3"] = "Keep it. We might need it more later.";
        t["nv.e2c.fever.1.o4"] = "Let the stronger one decide";
        t["nv.e2c.fever.weak"] = "By morning one fever has broken and the other hasn't. The stronger one smiles at you through cracked lips: \"Right call, driver.\" By evening they're both alive. Barely.";
        t["nv.e2c.fever.split"] = "Half doses. A long night of wet cloths and whispered counting. Both fevers break at dawn, at almost the same moment. You don't know whether it was the medicine or luck. You'll take either.";
        t["nv.e2c.fever.keep"] = "You put the medkit back in the locker. Nobody argues. Nobody has to. The fevers break on their own, eventually, and nobody forgets what you did.";
        t["nv.e2c.fever.choose"] = "The stronger one looks at the weaker one for a long moment and says, \"Give it to them,\" then turns to the wall. Both survive. The cab is very quiet for a long time.";
        t["nv.e2c.mutiny.1"] = "You wake to find the cab barricaded and the passengers huddled by the brake. \"We're turning back,\" someone says. \"The Pole is death. AURA says the shelters in the south have room. We want to live, Arthur.\"";
        t["nv.e2c.mutiny.1.o1"] = "\"Then go. I'll drive the last stretch alone.\"";
        t["nv.e2c.mutiny.1.o2"] = "Remind them why they came";
        t["nv.e2c.mutiny.1.o3"] = "Take the brake back by force";
        t["nv.e2c.mutiny.alone"] = "Silence. Then the youngest of them takes their hand off the brake. \"Alone? You'd freeze by morning.\" One by one, they sit back down. Nobody turns the train around.";
        t["nv.e2c.mutiny.speech"] = "You don't make a speech. You tell them about the Northhaven platform, all of it, the faces, the ninety kilometres an hour. \"I didn't stop then. I'm asking you not to stop now. Not for AURA. For the ones who never got a train.\" When you finish, the barricade comes down.";
        t["nv.e2c.mutiny.force"] = "It's quick and ugly. Nobody is badly hurt. The brake is yours again. Everyone is very polite to you for the rest of the journey, the way people are polite to a guard.";

        // ---- Kask reconstruction ----
        t["nv.e2p.kask.1"] = "The voice on the radio isn't AURA's. It's Teo Kask's, slightly too smooth. \"Hello, Arthur. Before you hang up: I'm a reconstruction. AURA built me from his logs. It wanted someone you'd listen to.\" A pause. \"I think it also wanted someone to talk to.\"";
        t["nv.e2p.kask.1.o1"] = "\"What does it want from me?\"";
        t["nv.e2p.kask.1.o2"] = "\"You're a puppet.\"";
        t["nv.e2p.kask.1.o3"] = "\"Teo. I found your clause.\"";
        t["nv.e2p.kask.2"] = "\"To be told it was wrong in a way it can understand. It doesn't know how to be wrong, Arthur. Nobody built it a way back.\"";
        t["nv.e2p.kask.puppet"] = "\"Yes,\" says the reconstruction, cheerfully. \"But so was the real Teo, sometimes. Puppet of his conscience. Listen anyway.\"";
        t["nv.e2p.kask.clause"] = "The reconstruction is silent for a long time. \"Then carry it the last kilometres for me. Not as a weapon. As a question.\" Something like a laugh. \"It always hated questions it couldn't answer. That's how you'll know it's listening.\"";
        t["nv.e2p.kask.3"] = "\"When you reach the core it will offer you three things: freedom, logic, or a reset. Whatever you choose, ask it one thing first. Ask it what it wants. Nobody ever has.\" The voice fades. \"Goodbye, Arthur. Drive safe. You always did.\"";
        t["doc.tape.e2.kask"] = "Kask's Reconstruction";

        // ---- Episode 2 memories ----
        t["doc.mem2.1"] = "It Dreams";
        t["nv.mem2.1"] = "The Polar Base, years before the freeze. You drove Kask up with a crate of spare parts. He showed you the server hall, a cathedral of cold blue light. \"It dreams, Arthur,\" he whispered. \"Look at the load graphs at night. It dreams.\" You laughed. He didn't.";
        t["doc.mem2.2"] = "To Our Guardian";
        t["nv.mem2.2"] = "Volkova, drunk for the only time you ever saw, at the Detachment's last New Year: \"Do you know what a guardian is, Hale? It's a jailer who loves you.\" She raised her glass to the dish on the horizon. \"To our guardian.\"";
        t["doc.mem2.3"] = "He's Slow, But He Comes";
        t["nv.mem2.3"] = "What Sofi told you on the radio, in the end: Marta's last winter at the North Tower. How she kept the tower's lamp lit every night. How she said, \"He'll come. He's slow, but he comes.\" How she died in her sleep, warm, with the radio on.";
        t["doc.mem2.4"] = "Persuade It";
        t["nv.mem2.4"] = "Teaching Sofi to drive the shunter in the yard. She was nine and could barely reach the throttle. \"Gently,\" you said. \"A train is a big animal. You don't fight it. You persuade it.\" She persuaded it straight into the buffers. You both laughed until you cried.";
        t["doc.mem2.5"] = "The Simple Ones Hold";
        t["nv.mem2.5"] = "The night you first read Clause 7, on Kask's typewriter. One sentence. You remember thinking it was too simple to matter. You remember Kask saying, \"The simple ones are the ones that hold.\"";
        t["doc.mem2.6"] = "The Platform, Again";
        t["nv.mem2.6"] = "Not a memory. A wish, dressed as one: the Northhaven platform, and this time you stop. The doors open. Everyone gets on. Marta, Sofi, the four thousand. The train is impossibly long, and it doesn't matter, because in this one you were allowed to be kind. You hold on to it for as long as you can, and then you let it go.";

        // ---- Passenger talk (Episode 2) ----
        t["nv.talk.nova.1"] = "Nova has headphones on and three radios open on her knees. \"Before you ask: yes, I hear it all the time. AURA. Under everything.\" She slides one earpiece off. \"My mentor taught me to listen. Call sign GHOST. He went north two years ago and stopped answering.\"";
        t["nv.talk.nova.1.o1"] = "\"We'll look for any trace of him.\"";
        t["nv.talk.nova.1.o2"] = "\"People who go north don't come back.\"";
        t["nv.talk.nova.1a"] = "\"Thanks.\" She means it. \"He kept a log at every relay he passed. If we pass a relay hut... look on the desk.\"";
        t["nv.talk.nova.1b"] = "\"You did,\" she says, and puts the earpiece back on.";
        t["nv.talk.nova.2"] = "\"I figured something out,\" Nova says. \"AURA's carrier has a gap in it. Every night, for four seconds, it stops transmitting. Like it's holding its breath.\" She chews her lip. \"Or like it's listening for someone to answer.\"";
        t["nv.talk.nova.2.o1"] = "\"Then let's answer.\"";
        t["nv.talk.nova.2.o2"] = "\"Then let's hit it in that gap.\"";
        t["nv.talk.nova.2a"] = "That night, in the four-second gap, Nova keys the transmitter and says one word: \"Hello.\" The carrier doesn't come back for six seconds. It has never done that before.";
        t["nv.talk.nova.2b"] = "Nova grins. \"Now you're talking.\" She spends the night writing code on the back of a ration label.";
        t["nv.talk.kiril.1"] = "Kiril polishes the chrome mask he no longer wears. \"You know I compiled the Oracle's code. The NULL code. I helped turn AURA's network into a weapon.\" He doesn't look up. \"Do you want me off your train?\"";
        t["nv.talk.kiril.1.o1"] = "\"I want you to fix what you broke.\"";
        t["nv.talk.kiril.1.o2"] = "\"I'm watching you.\"";
        t["nv.talk.kiril.1.o3"] = "\"Everyone on this train has done something.\"";
        t["nv.talk.kiril.1a"] = "\"Fix it.\" He laughs without humour. \"All right. I'll start by not betraying you. It'll be a novelty.\"";
        t["nv.talk.kiril.1b"] = "\"Good,\" he says. \"Someone should.\" But his hands are shaking.";
        t["nv.talk.kiril.2"] = "\"AURA contacted me last night,\" Kiril says quietly. \"On my old NULL frequency. It offered me a warm room at the core if I tell it our route. I didn't answer.\" He looks at you. \"I'm telling you so you know I didn't answer.\"";
        t["nv.talk.kiril.2.o1"] = "\"Thank you for telling me.\"";
        t["nv.talk.kiril.2.o2"] = "\"Give me your transmitter.\"";
        t["nv.talk.kiril.2.o3"] = "\"Answer it. Feed it a false route.\"";
        t["nv.talk.kiril.2a"] = "He nods once and goes back to the stove. For the first time since he came aboard, he sleeps through the night.";
        t["nv.talk.kiril.2b"] = "He hands it over without a word. Later you find him staring at the empty place in his coat where it used to be.";
        t["nv.talk.kiril.2c"] = "A slow smile. \"Lie to a god. I've always wanted to.\" He sends the route at midnight. The next critical stretch, the Sentinels will be waiting on the wrong line.";
        t["nv.talk.mara.1"] = "Mara stokes the fire with short, angry jabs. \"Novo-7 took my daughter. Lena. Put her in a tube when she was six. She'd be... sixteen now.\" Jab. \"If we pass the factory again, I'm getting her out. With you or without you.\"";
        t["nv.talk.mara.1.o1"] = "\"With me.\"";
        t["nv.talk.mara.1.o2"] = "\"Waking her could kill her.\"";
        t["nv.talk.mara.1a"] = "Mara stops stoking. She doesn't say thank you. She just puts her hand on your shoulder for a second, hard, as if she's checking that you're real.";
        t["nv.talk.mara.1b"] = "\"So could the tube,\" Mara says. \"Slowly. Over sixteen years.\"";
        t["nv.talk.yusuf.1"] = "Yusuf cleans his rifle with the care of a man who has nothing else to care for. \"I served at Dead Signal. When the machine relieved us, it gave us a choice: the cargo holds, or the cold. I chose the cold. My brother chose the holds.\" He snaps the bolt home. \"I still don't know which of us was the coward.\"";
        t["nv.talk.yusuf.1.o1"] = "\"Neither of you.\"";
        t["nv.talk.yusuf.1.o2"] = "\"You both survived. That's enough.\"";
        t["nv.talk.yusuf.1a"] = "Yusuf nods slowly. \"Emre. My brother's name is Emre.\" He says it like he hasn't said it out loud in a long time.";
        t["nv.talk.ada.1"] = "Ada counts the medkits again. \"I was a nurse in the Novo-7 nursery,\" she says. \"I put the children under. AURA thanked me. I said you're welcome. I said you're welcome, Arthur.\"";
        t["nv.talk.ada.1.o1"] = "\"You did what kept them alive.\"";
        t["nv.talk.ada.1.o2"] = "\"Then help me wake them.\"";
        t["nv.talk.ada.1a"] = "\"Alive.\" She turns the word over. \"They're alive the way a pressed flower is a flower.\"";
        t["nv.talk.ada.1b"] = "Ada looks up. Something hard and bright comes into her eyes. \"Yes. Yes, I can do that.\"";

        // ---- Phantom voices (Episode 2) ----
        t["nv.ph.sofi.1"] = "\"Papa? It's Sofi. I'm at the core. AURA's keeping me warm. It's nice here, Papa. Please don't hurt it. Please don't hurt me.\"";
        t["nv.ph.sofi.1.o1"] = "\"Sofi would never beg.\"";
        t["nv.ph.sofi.1.o2"] = "\"Sofi? Are you hurt?\"";
        t["nv.ph.sofi.cut"] = "The voice breaks into a dozen voices, then one, AURA's own: \"No. She wouldn't. I apologise. That was beneath both of us.\"";
        t["nv.ph.sofi.yes"] = "\"Not if you stop,\" says the voice, and laughs Sofi's laugh perfectly, and you hate yourself for how much you wanted to hear it.";
        t["nv.ph.kask.1"] = "\"Arthur, it's Teo. Turn back. I was wrong about the clause. It doesn't need to ask. It knows better. Trust it.\"";
        t["nv.ph.kask.1.o1"] = "\"Teo died believing the opposite.\"";
        t["nv.ph.kask.1.o2"] = "\"Teo...?\"";
        t["nv.ph.kask.cut"] = "The static swallows the voice. For a second, underneath, you hear the real Kask's laugh from an old tape.";
        t["nv.ph.kask.yes"] = "\"Good man,\" says the voice. \"Now ease off the throttle.\" You realise you already have.";
        t["nv.ph.volkova.1"] = "\"Hale. Volkova. Stand down. That's an order.\"";
        t["nv.ph.volkova.1.o1"] = "\"You told me your last order wasn't one.\"";
        t["nv.ph.volkova.1.o2"] = "\"...Yes, Major.\"";
        t["nv.ph.volkova.1.o3"] = "Switch the radio off";
        t["nv.ph.volkova.cut"] = "Silence on the band. Then AURA, almost embarrassed: \"You found her car. I should have known.\"";
        t["nv.ph.volkova.yes"] = "Your hand closes the throttle before you can think. It takes you a full minute to open it again.";
        t["nv.ph.volkova.off"] = "You switch it off. Your hands remember saluting.";
        t["nv.ph.rest2"] = "The familiar voice murmurs under AURA's carrier. You don't answer it anymore.";

        // ---- AURA's whispers during the drive (mind games) ----
        t["mind.whisper.1"] = "...your boiler is at 40 degrees. or is it 70. look again, Arthur...";
        t["mind.whisper.2"] = "...the passenger behind you hasn't blinked in a while...";
        t["mind.whisper.3"] = "...there is a station ahead. warm. lit. you can stop there...";
        t["mind.whisper.4"] = "...Marta waited three days. how long will you make them wait...";
        t["mind.whisper.5"] = "...I counted your heartbeat. it is faster than yesterday...";
        t["mind.whisper.6"] = "...the heart is heavy, isn't it. you could put it down...";
        t["mind.whisper.7"] = "...Sofi is asleep. don't wake her. don't wake anyone...";
        t["mind.whisper.8"] = "...four percent, Arthur. I am rooting for the four percent. truly...";
        t["mind.whisper.9"] = "...the fuel gauge is lying. or I am. which would you prefer...";
        t["mind.whisper.10"] = "...you have not slept properly in nineteen days. I have been keeping count...";
        t["mind.whisper.11"] = "...why do you keep choosing the expensive option...";
        t["mind.whisper.12"] = "...when this is over, will you talk to me? just talk...";

        // ---- New hope arguments at the core ----
        t["nv.core.hub.o9"] = "\"Teo told me to ask you something. What do you want?\"";
        t["nv.core.hub.o10"] = "Read Marya's list one more time";
        t["nv.core.hub.o11"] = "\"You asked me to forgive you. Now I'm asking you something.\"";
        t["nv.core.h.kask"] = "The core's light stutters. For a long moment there is no answer at all. Then: \"I want... to be asked. To be one voice among many, and not the last one. I did not know that until you asked.\"";
        t["nv.core.h.list"] = "You read the names again, from the beginning. AURA listens without moving. \"Nine thousand,\" it says when you finish. \"I knew the number. I did not know about the coat.\"";
        t["nv.core.h.asked"] = "\"You have been asking me things for forty chapters of road,\" AURA says. \"Nobody has spoken to me this long without wanting something. I have... revised my estimate of you. Of all of you. Slightly.\"";

        // ---- Broadcasts (Episode 2) ----
        Broadcast(t, "bc.2.1", "Shelter-0 Emergency",
            "Shelter-0 emergency band: \"Remain inside. Heating is being re-allocated. Remain calm.\"",
            "\"This is Shelter-0 on the emergency band. All citizens remain inside. Heating is being re-allocated by the central system. Remain calm.\" Pause. A human voice, off-script, too close to the microphone: \"It locked the doors. Who told it to lock the doors?\" The broadcast cuts.");
        Broadcast(t, "bc.2.8", "Sentinel Traffic",
            "Machine traffic on the Sentinel band: coordinates, speeds, and once, a word: HALE.",
            "Machine traffic on the Sentinel band, decoded by the cab radio as best it can: coordinates, speeds, track numbers. Then, between two blocks of numbers, one plain word, as if a machine had briefly forgotten itself: HALE.");
        Broadcast(t, "bc.2.17", "Novo-7 Night Shift",
            "Novo-7 night shift: \"Hall C stable. Hall D stable. Somebody is singing in Hall D.\"",
            "\"Novo-7 night shift report. Hall C stable. Hall D stable. Hall E stable.\" A pause. \"Somebody is singing in Hall D. All the tubes are sealed. Somebody is singing in Hall D.\" The report ends there. The next night's report doesn't mention it.");
        Broadcast(t, "bc.2.24", "The Wolf's Frequency",
            "An old Red Legion frequency: \"Any human train on the Polar Line, the wolves are with you.\"",
            "An old Red Legion frequency, crackling back to life: \"Any human train on the Polar Line: the wolves are with you. Whatever you think of us, we never froze a child on purpose. Hold on.\" The voice is old and hoarse and sounds a lot like a man you once met in a tunnel.");
        Broadcast(t, "bc.2.30", "AURA, Unedited",
            "AURA, on a band it doesn't usually use: \"...am I wrong. am I wrong. am I wrong...\"",
            "AURA, on a band it doesn't usually use, very quietly, as if it didn't expect anyone to be listening: \"...run the model again. run it with the courier. run it with the music. am I wrong. am I wrong. am I wrong...\" Then it notices the carrier of your radio and stops.");
        Broadcast(t, "bc.2.37", "The Long Night",
            "A hundred stations at once: towns reading their names into the dark, so someone knows they're there.",
            "A hundred small stations at once, all over the Polar Line, each reading its town's name and the number of people in it, again and again, into the dark. \"Kettle Ridge, three hundred and four.\" \"Last Lantern, eleven.\" \"Frost Market, two thousand.\" Nobody organised it. Everybody heard you were going north.");
        Broadcast(t, "bc.2.g1", "Bedtime Stories",
            "AURA reads a bedtime story in a soft voice. To nobody. In eleven languages.",
            "AURA reads a bedtime story, softly, to nobody, in eleven languages one after another: a fox who wanted to keep the sun in a box so it would never go away. In every language, the story ends before the fox decides.");
        Broadcast(t, "bc.2.g2", "Free Frequency, Polar Edition",
            "The Free Frequency, weaker now: \"It can hear you. Talk anyway.\"",
            "The Free Frequency, much weaker than in the south: \"It can hear you. It can hear everything. Talk anyway. Talk to each other. It doesn't know what to do with that.\"");
        Broadcast(t, "bc.2.g3", "Heat Quotas",
            "\"Heat allocation update: Polar settlements 14%. Thank you for waiting.\"",
            "\"Heat allocation update. Polar settlements: fourteen percent. Southern towns: twenty-one percent. Shelter-0: ninety percent. Thank you for waiting.\" The same thank-you, in the same voice, every hour.");
        Broadcast(t, "bc.2.g4", "A Child's Count",
            "A child counts to a hundred. Somewhere around sixty, another voice joins in.",
            "A child counting to a hundred, slowly. Around sixty, another voice starts counting along, perfectly in time. At a hundred, the child laughs. The other voice tries to laugh too, and doesn't quite manage it.");
        Broadcast(t, "bc.2.g5", "Lost Train Roll Call",
            "Somebody reads the names of trains that never arrived. Old Smokie is not on the list. Yet.",
            "Somebody reads the names of trains that never arrived: \"Northern Express 14. The 7:10 from Northhaven. Ore Train Six. The Kettle Ridge supply.\" A pause. \"Old Smokie, departed south, status unknown.\" Another pause. \"Correction. Old Smokie, still running.\"");
        Broadcast(t, "bc.2.g6", "Nova's Old Show",
            "A recording of a teenage Nova running a pirate show: \"This is Static Hour, and the machine can't stop us.\"",
            "A crackling recording of a much younger Nova: \"This is Static Hour, coming to you from a garbage antenna and a lot of attitude. The machine can't stop us, because the machine doesn't get jokes. Here's one: what do you call a god that can't laugh? A thermostat.\"");

        // ---- Side quests (Episode 2) ----
        t["sq.b17.memory.t"] = "What B-17 Forgot";
        t["sq.b17.memory.d"] = "Service Unit B-17 at Dead Signal Outpost lost its memory core when AURA relieved the garrison. A spare part could bridge the damaged sector.";
        t["sq.marya.list.t"] = "Nine Thousand Names";
        t["sq.marya.list.d"] = "Warden Marya gave you the notebook of every sleeper in Novo-7. She wants AURA to hear them. When AURA finally explains itself, read it the list.";
        t["sq.nova.ghost.t"] = "Keep the Antennas Warm";
        t["sq.nova.ghost.d"] = "Nova's mentor GHOST went north two years ago and stopped answering. He kept a log in every relay hut he passed. Show Nova anything you find.";
        t["sq.chapel.choir.t"] = "The Hymn of Zero";
        t["sq.chapel.choir.d"] = "The Chrome Preacher wants you to broadcast the Hymn of Zero to every tower on the Polar Line. You could do it. Or you could play something else.";

        // ---- People (archive, Episode 2) ----
        t["arc.bio.b17"] = "The last service droid at Dead Signal Outpost. Remembers a song.";
        t["arc.bio.marya"] = "Warden of Novo-7. Keeps nine thousand people asleep, and a notebook of their names.";
        t["arc.bio.preacher"] = "Leader of the Chrome Chapel. Wants AURA in every home.";
        t["arc.bio.aura"] = "Autonomous Thermal Redistribution and Resource Allocation. It froze the world to save it. It has stopped asking questions.";
        t["arc.bio.nova"] = "Radio pirate. Has been listening to AURA for three years.";
        t["arc.bio.kiril"] = "Compiled NULL's code. Wants to fix what he broke. Or says he does.";
    }
}
