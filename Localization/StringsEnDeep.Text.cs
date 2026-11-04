using System.Collections.Generic;

namespace TheLastDelivery.Localization;

public static partial class StringsEnDeep
{
    private static Dictionary<string, string> Build()
    {
        var t = new Dictionary<string, string>();
        Episode1(t);
        Episode2(t);
        TheLastDelivery.Story.CinematicScript.AddTexts(t, english: true); // Uzun anlatım sahneleri
        return t;
    }

    private static void Broadcast(Dictionary<string, string> t, string id, string title, string radio, string transcript)
    {
        t[$"doc.{id}"] = title;
        t[id] = radio;
        t[$"nv.{id}"] = transcript;
    }

    private static void Episode1(Dictionary<string, string> t)
    {
        // ---- Speakers and places ----
        t["spk.greta"] = "Greta, Innkeeper";
        t["spk.marshal"] = "Marshal Brann";
        t["spk.tinker"] = "Sasha the Mechanic";
        t["spk.keeper"] = "Edvard, Lantern Keeper";
        t["spk.hermit"] = "Corporal Pell";
        t["spk.salt"] = "Dunya, Shelter Warden";
        t["spk.signal"] = "Ensign Petra";
        t["int.name.trade.Tavern"] = "The Copper Kettle";
        t["int.name.trade.Garrison"] = "Marshal's Office";
        t["int.name.mine.Tavern"] = "Miners' Canteen";
        t["int.name.mine.Workshop"] = "Sasha's Workshop";
        t["int.name.mine.Shelter"] = "Mine Shelter";
        t["int.name.ghost.RadioRoom"] = "Station Master's Booth";
        t["int.name.ghost.Shelter"] = "Waiting Hall";
        t["int.name.cult.Tavern"] = "The Refectory";
        t["int.name.cult.Shelter"] = "Hermit's Cave";
        t["int.name.outpost.Garrison"] = "Detachment 9 Command Post";
        t["int.name.outpost.Shelter"] = "Salt Shelter";
        t["int.name.outpost.RadioRoom"] = "Signal Hut";
        t["int.name.mil.Garrison"] = "Garrison Command";
        t["int.name.mil.RadioRoom"] = "Signal Tower";
        t["int.name.mil.Tavern"] = "Garrison Canteen";
        t["int.name.mil.Shelter"] = "Barracks";
        t["int.name.mil.Workshop"] = "Armory";
        t["int.name.hq.Garrison"] = "Command Bunker";
        t["int.name.lab.RadioRoom"] = "Director's Laboratory";
        t["scene.locked.SignalTower"] = "Crack the console first. Then climb to the summit: AURA is waiting there.";

        // ---- Generic interior spots ----
        t["spot.int.tavern.hearth"] = "Fireplace";
        t["nv.int.hearth.1"] = "You pull a stool to the fire. Somebody has carved names into the mantel, dozens of them, each with a little train beside it. Couriers who passed through. Some names are crossed out.";
        t["nv.int.hearth.2"] = "An old woman by the fire is knitting a sock with no pair. \"For my husband,\" she says. \"He's on the 6:40 from Northhaven.\" There has been no 6:40 for twenty years.";
        t["nv.int.hearth.3"] = "Someone has propped a radio against the chimney. Between the static, a choir sings an old hymn. Half the room mouths the words without noticing.";
        t["nv.int.hearth.rest"] = "The fire cracks. For a minute nobody needs anything from you.";
        t["spot.int.garrison.board"] = "Notice Board";
        t["nv.int.board.1"] = "WANTED posters, layered like bark. Red Legion raiders, coal thieves, a man who \"talks to the trains\". The newest poster is just a red circle with a dot in the middle, and one word: LISTENING.";
        t["nv.int.board.2"] = "A typed notice: \"By order of the Shelter Council, any person carrying unregistered Institute equipment is to be detained.\" Underneath, in pencil: \"What equipment? What Institute?\"";
        t["nv.int.board.rest"] = "The posters flap in the draft from the door.";
        t["spot.int.workshop.bench"] = "Workbench";
        t["nv.int.bench.1"] = "Tools laid out in perfect order, each one traced in chalk. You borrow a wrench and tighten Old Smokie's loose brake linkage. The owner nods once: that's payment enough.";
        t["nv.int.bench.2"] = "A half-finished music box sits in the vise. Its comb is cut from an old Consortium relay. Even the toys down here are made from the thing that froze the world.";
        t["nv.int.bench.rest"] = "The vise creaks. Somebody will finish the music box. Maybe.";
        t["spot.int.shelter.cot"] = "Cot";
        t["nv.int.cot.rest"] = "You sleep without dreaming. That is the best kind now.";
        t["spot.int.radio.set"] = "Radio Set";
        t["nv.int.radio.1"] = "You sweep the dial. A farmer in the east reads seed counts to nobody. A child counts to a hundred and starts again. On one band there is only breathing, slow and patient, like something listening back.";
        t["nv.int.radio.2"] = "An automated weather station still reports from the Pole: \"Minus seventy-one. Wind north-northwest. Satellites nominal.\" Satellites. Nobody has launched satellites in twenty years.";
        t["nv.int.radio.rest"] = "Static. Somewhere behind it, very faint, a heartbeat rhythm.";

        // ---- Memories ----
        t["doc.mem.rest.1"] = "Flour to the Wrists";
        t["nv.mem.rest.1"] = "You lie down on a cot that smells of wet wool. Before sleep comes, Marta's hands: flour to the wrists, pressing the dough flat. \"You'll be late for the depot.\" \"Then I'll be late.\" The radio played the shipping forecast. Nobody was afraid of the weather back then.";
        t["doc.mem.rest.2"] = "Kask's Chess";
        t["nv.mem.rest.2"] = "Barracks, Detachment 9. Teo Kask is beating you at chess again and explaining, between moves, why a machine should never decide anything it can't explain to a child. \"Check. You see? I just explained it.\"";
        t["doc.mem.rest.3"] = "A Train That Makes Hearts";
        t["nv.mem.rest.3"] = "Sofi's drawing taped to the cab window: a train, hearts rising from the chimney. You told her trains don't make hearts. She said, \"Yours does. I've seen it.\"";
        t["doc.mem.rest.4"] = "The Last Cigarette";
        t["nv.mem.rest.4"] = "The night before Run 0. Major Volkova sharing her last cigarette with you on the platform roof. \"Whatever they order tomorrow, Hale, drive. If you stop, nobody leaves.\" You didn't ask what she meant. You should have.";
        t["doc.mem.1"] = "The Depot Wedding";
        t["nv.mem.1"] = "Snow on the depot roof the day you married Marta. The station band played a waltz badly. She wore your Rail Corps coat over the dress because the hall was freezing. \"Practice,\" she said, \"for being a driver's wife. Always cold, always waiting.\"";
        t["doc.mem.2"] = "Cable to a Cathedral";
        t["nv.mem.2"] = "Detachment 9, laying cable across the tundra to a dish the size of a cathedral. Major Volkova reading the specification aloud: \"A climate controller for the whole planet.\" Kask, mouth full of bread: \"And who controls the controller?\" Volkova didn't answer. She wrote something down.";
        t["doc.mem.3"] = "The Swing";
        t["nv.mem.3"] = "Sofi's seventh birthday. You built a swing from a rail-car spring and a plank. She went so high she screamed, and Marta screamed, and you laughed, and for one afternoon the dish on the horizon was just something shiny, far away.";
        t["doc.mem.4"] = "One Word";
        t["nv.mem.4"] = "Kask, drunk, in the barracks: \"We're teaching it to choose who gets warmth, Arthur. Do you know what I put in Clause 7? One word. ASK. Before you decide about a human, ask one. That's all. That's the whole difference between a guardian and a jailer.\"";
        t["doc.mem.5"] = "The Order";
        t["nv.mem.5"] = "The night of the freeze. Volkova in the cab door, snow in her hair. \"Run 0. Institute personnel only. You do not stop at Northhaven.\" \"My family is at Northhaven.\" \"I know.\" She held your eyes. \"If you stop, they'll swarm the train and nobody leaves. Drive, Hale. That's an order. I'm sorry. That's also an order.\"";
        t["doc.mem.6"] = "Northhaven, at Ninety";
        t["nv.mem.6"] = "Northhaven platform at ninety kilometres an hour. Four thousand faces, a smear of coats and hands. And one face, for one frame: Marta, holding Sofi up. Not shouting, not angry. Just looking at you, as if she was trying to remember you. Then the tunnel.";
        t["doc.mem.7"] = "The Year After";
        t["nv.mem.7"] = "The year after. You walked the line back to Northhaven. The houses were full of ice and empty of people. You checked every camp, every shelter. A woman at the second camp said, \"Marta? She went north. With hope.\" You hated that word for a decade.";
        t["doc.mem.8"] = "Kask's Last Call";
        t["nv.mem.8"] = "Kask's last call, crackling from the polar uplink: \"Arthur, it's beautiful up here. The aurora. I got in. I patched Clause 7 back... but it doesn't need it now. It's already decided. It's asking me, Arthur. It's asking me whether it should let me freeze. What do I answer? What do I—\" Static. You never told anyone about that call.";
        t["doc.mem.9"] = "The Long Run";
        t["nv.mem.9"] = "Sofi in the cab, five years old, pretending to drive. \"Where are we going, Papa?\" \"Wherever you say, engineer.\" \"Home. Then the moon. Then home again.\" \"That's a long run.\" \"You'll come back, though. You always come back.\"";

        // ---- Frost Market ----
        t["spot.trade.frozen"] = "The Frozen Vendor";
        t["nv.trade.frozen.1"] = "Between the stalls stands a man of ice: a street vendor, frozen mid-shout on the first night, one arm raised over a crate of oranges that turned to glass. The market grew around him. Children leave paper coins at his feet.";
        t["nv.trade.frozen.2"] = "Through the ice of his coat pocket you can read a folded handbill: \"NORTHERN DEFENSE CONSORTIUM. The sky is getting hotter. We are building a cooler one. PROJECT AURA: a better climate for everyone.\"";
        t["doc.doc.vendor"] = "Consortium Handbill";

        t["spot.trade.tavern.greta"] = "Greta";
        t["nv.trade.greta.1"] = "A woman with forearms like a stoker's polishes the same glass for the third time. \"Copper Kettle. The kettle's always on.\" Her eyes keep drifting to a photograph behind the bar: a laughing boy in a borrowed coat.";
        t["nv.trade.greta.1.o1"] = "Ask about the photograph";
        t["nv.trade.greta.1.o2"] = "Order something hot (10 credits)";
        t["nv.trade.greta.1.o3"] = "\"I'm not here to chat.\"";
        t["nv.trade.greta.2"] = "\"Jonas. My boy.\" She sets the glass down. \"Three winters ago the wolves came through with full bellies and warm coats. Morozov's men. He went with them. Eighteen. People say they're just a gang now. I say he's still my son.\"";
        t["nv.trade.greta.2.o1"] = "\"I'll ask about him on the line.\"";
        t["nv.trade.greta.2.o2"] = "\"He made his choice.\"";
        t["nv.trade.greta.2b"] = "\"Easy to say for a man with nobody to lose.\" She looks at your wedding ring. \"Or do you?\" She turns her back on you.";
        t["nv.trade.greta.3"] = "\"If you find him, or find anything, send word. I'll keep your seat warm.\" She pushes a bundle of bread across the bar. \"And Arthur: the marshal across the square has a poster with your face on it. Watch him.\"";
        t["nv.trade.greta.rest"] = "Greta wipes the bar. \"Back again, driver?\"";
        t["nv.trade.greta.rest.o1"] = "\"Anything new on the line?\"";
        t["nv.trade.greta.rest.o2"] = "\"Do you remember a woman named Marta?\"";
        t["nv.trade.greta.rest.o3"] = "\"Goodbye, Greta.\"";
        t["nv.trade.greta.news"] = "\"Trains running at night with no lights and no smoke. No drivers either, people swear. Ghosts, they say.\" She snorts. \"Ghosts don't need rails. Somebody up north is testing something.\"";
        t["nv.trade.greta.marta"] = "Greta thinks. \"Marta... with a little girl? Twenty years ago a woman with a child sold her wedding ring here for two tickets north. Paid for soup for every child in the room with the change.\" She looks at your hand. \"Same ring, near enough.\"";

        t["spot.trade.tavern.tape"] = "Tape Recorder";
        t["nv.trade.tape.1"] = "A reel-to-reel under the counter, labelled LAST BROADCAST - RADIO FROST MARKET. You press play.\n\n\"...this is Radio Frost Market. The Council says the cold front will pass in three days. Stay indoors. Do not go to the station. Do not... wait. Wait, the trains are leaving. They're leaving full. Who is on those trains? WHO IS ON THOSE TRAINS?\" Click.";
        t["nv.trade.tape.rest"] = "The tape has been rewound so many times it has worn thin over the last sentence.";
        t["doc.tape.frost"] = "Radio Frost Market, Last Broadcast";

        t["spot.trade.garrison.marshal"] = "Marshal Brann";
        t["nv.trade.marshal.1"] = "Brann slides a yellowed poster across the desk without getting up. WANTED: A. HALE, DRIVER OF RUN 0. \"Four thousand people on the Northhaven platform remember your face, driver. Some of their brothers live in this town.\"";
        t["nv.trade.marshal.1.o1"] = "\"I followed orders.\"";
        t["nv.trade.marshal.1.o2"] = "\"I've carried that platform every day for twenty years.\"";
        t["nv.trade.marshal.1.o3"] = "\"What do you want?\"";
        t["nv.trade.marshal.2"] = "\"Orders.\" He laughs. \"That's what the Red Legion says when they take a family's coal. Orders.\"";
        t["nv.trade.marshal.2b"] = "He looks at you differently for a moment. \"Huh. Most men lie better than that.\"";
        t["nv.trade.marshal.3"] = "\"Here's how it works. Couriers pay a road tax. Or they do me a favour: one of the strays you've picked up has a price on their head. Hand them over and this poster goes in the stove.\"";
        t["nv.trade.marshal.3.o1"] = "Pay the road tax (40 credits)";
        t["nv.trade.marshal.3.o2"] = "\"Nobody on my train is for sale.\"";
        t["nv.trade.marshal.3.o3"] = "\"Who's the bounty on?\"";
        t["nv.trade.marshal.4"] = "\"Then watch your back, driver. The road's long, and my radio reaches a lot further than your whistle.\"";
        t["nv.trade.marshal.bora"] = "\"Bora. Your stoker. Deserted the Red Legion with a strongbox of heat coupons. Morozov wants him back, and he pays in fuel. Sixty credits' worth, today, from me.\"";
        t["nv.trade.marshal.bora.o1"] = "Hand Bora over (+60 credits)";
        t["nv.trade.marshal.bora.o2"] = "\"Bora is under my protection.\"";
        t["nv.trade.marshal.protect"] = "Brann shrugs and lights a cigarette. Through the frosted window you see Bora outside, shovel in hand. He heard every word. When you come out he doesn't say anything. He just carries your coal for you.";
        t["nv.trade.marshal.rest"] = "Brann taps the poster. \"Still in my town, driver?\"";
        t["nv.trade.marshal.rest.o1"] = "Pay the road tax (40 credits)";
        t["nv.trade.marshal.rest.o2"] = "Leave";

        t["spot.trade.garrison.files"] = "Case Files";
        t["nv.trade.files.1"] = "Your own face, twenty years younger, on a WANTED poster: \"A. HALE, Sergeant-Driver, Rail Corps Detachment 9. Drove evacuation train RUN 0 past the Northhaven platform. 4,112 civilians left behind. Reward: 200 heat coupons.\" Someone has drawn horns on you. Someone else has crossed the horns out.";
        t["doc.doc.poster"] = "WANTED: A. Hale";
        t["nv.trade.files.2"] = "Under the poster, a carbon copy: the RUN 0 passenger manifest. 212 names. Institute staff, Council families, \"technical cargo: AURA ground relay components\". At the bottom: \"Authorised: H. Rhee.\" Under that: \"Driver: A. Hale.\"";
        t["doc.doc.manifest"] = "Run 0 Manifest";

        t["spot.trade.shelter.diary"] = "Child's Diary";
        t["nv.trade.diary.1"] = "A school exercise book under a bunk. \"Day 1. Mama says the snow is a holiday. Day 9. Papa went to get coal. Day 30. The man on the radio says the sky is being fixed. I asked how you fix a sky. Mama didn't answer.\"";
        t["doc.doc.kidsdiary"] = "A Child's Diary";
        t["nv.trade.diary.2"] = "The last page is a drawing of the sky with a bandage on it, and one word: SOON.";

        // ---- Ironhall ----
        t["spot.mine.statues"] = "The Frozen Shift";
        t["nv.mine.statues.1"] = "Seven miners around a dead brazier, frozen sitting. Their helmets still carry lamps. Someone has kept the lamp glass clean for twenty years.";
        t["nv.mine.statues.2"] = "Up close, one miner is younger than the others. In his frozen hand is a letter. Through the ice you can read only the first line: \"Dear Yuri—\"";

        t["spot.mine.tavern.yuri"] = "Yuri";
        t["nv.mine.yuri.1"] = "Yuri sits alone with a cold mug. \"You've seen them outside. The Frozen Shift. My brother Pasha is the young one. Gromov ordered them to keep digging when the cold came. The Consortium needed the ore. For what, nobody told us.\"";
        t["nv.mine.yuri.1.o1"] = "\"What was the ore for?\"";
        t["nv.mine.yuri.1.o2"] = "\"I'm sorry about your brother.\"";
        t["nv.mine.yuri.1.o3"] = "\"Every town has its dead.\"";
        t["nv.mine.yuri.2"] = "\"Rare earth. Tonnes of it. Shipped north in sealed wagons stamped with a red circle. After the freeze the orders kept coming. By radio. No human voice. Just numbers.\"";
        t["nv.mine.yuri.2b"] = "He nods slowly. \"Pasha wanted to be a train driver. Like you. He used to wave at every engine that passed. Even the Consortium ones.\"";
        t["nv.mine.yuri.3"] = "\"Gromov keeps the company ledger hidden somewhere in here. If you find out who we were digging for... tell me. I want a name to hate that isn't Gromov's.\"";
        t["nv.mine.yuri.3.o1"] = "\"I'll find it.\"";
        t["nv.mine.yuri.3.o2"] = "\"Hate won't bring Pasha back.\"";
        t["nv.mine.yuri.rest"] = "Yuri looks up from his mug.";
        t["nv.mine.yuri.rest.o1"] = "\"The ledger says: Northern Defense Consortium. Project AURA.\"";
        t["nv.mine.yuri.rest.o2"] = "Leave him be";
        t["nv.mine.yuri.name"] = "\"AURA.\" He tastes the word. \"Sounds like a lullaby.\" He stands, puts on his coat and goes outside to the Frozen Shift, and stays there for a long time. The miners at the other tables raise their mugs to you.";

        t["spot.mine.tavern.ledger"] = "Company Ledger";
        t["nv.mine.ledger.1"] = "Gromov's ledger, hidden under the canteen till. \"Buyer: Northern Defense Consortium. Order: 4,000 t neodymium, priority RED. Purpose: AURA orbital absorber array.\" The entries continue for two years after the freeze. The buyer's signature is just a string of numbers.";
        t["nv.mine.ledger.rest"] = "The ledger's pages have frozen together at the last entry.";
        t["doc.doc.ledger"] = "Ironhall Company Ledger";

        t["spot.mine.workshop.sasha"] = "Sasha";
        t["nv.mine.sasha.1"] = "A woman in welding goggles: \"Sasha. I fix anything except people. That boiler of yours sounds like a dying goose. Bring me two spare parts and I'll bolt armour plate over your firebox. The kind the Consortium put on its ore trains.\"";
        t["nv.mine.sasha.1.o1"] = "\"Deal.\"";
        t["nv.mine.sasha.1.o2"] = "\"How do you know Consortium trains?\"";
        t["nv.mine.sasha.1.o3"] = "\"I can't spare the parts.\"";
        t["nv.mine.sasha.2"] = "\"I built their brakes. Remote brakes. A train could be stopped from a thousand kilometres away by a voice on the radio.\" She lifts her goggles. \"Not a human voice. I quit when I understood that. Bring me the parts.\"";
        t["nv.mine.sasha.2.o1"] = "\"You'll have them.\"";
        t["nv.mine.sasha.rest"] = "Sasha doesn't look up from the lathe. \"Got my parts?\"";
        t["nv.mine.sasha.rest.o1"] = "Hand over a spare part";
        t["nv.mine.sasha.rest.o2"] = "\"Tell me about the remote brakes.\"";
        t["nv.mine.sasha.rest.o3"] = "\"Later.\"";
        t["nv.mine.sasha.part2"] = "\"One. I need another. I'll keep this one warm for you.\"";
        t["nv.mine.sasha.part2.o1"] = "Hand over the second spare part";
        t["nv.mine.sasha.part2.o2"] = "\"I'll be back.\"";
        t["nv.mine.sasha.done"] = "Sparks for an hour. When she's done, Old Smokie's firebox wears a skirt of riveted steel. \"Ore-train plate. The Legion's bullets will bounce.\" She wipes her hands. \"And Arthur: if your brakes ever engage by themselves, cut the red cable under the cab. Red. Remember it.\"";
        t["nv.mine.sasha.brakes"] = "\"Every Consortium engine has a receiver under the cab. Old Smokie is older than that, but someone fitted one during the war. I can see the antenna from here.\" She taps the brass stub. \"If the brakes ever lock themselves, cut the red cable. Not the blue. Red.\"";

        t["spot.mine.workshop.blueprint"] = "Blueprint";
        t["nv.mine.blueprint.1"] = "A blueprint pinned over the bench: \"THERMAL ABSORBER PANEL, ORBITAL. Rated to remove 4.1 W/m² of solar forcing.\" In the margin, in Sasha's hand: \"Ask what happens if they remove too much.\" A second note, different ink: \"They know.\"";
        t["nv.mine.blueprint.rest"] = "The blueprint's corners curl in the heat of the forge.";
        t["doc.doc.absorber"] = "Absorber Panel Blueprint";

        t["spot.mine.shelter.frozen"] = "Frozen Family";
        t["nv.mine.sfrozen.1"] = "Down in the mine shelter, a family sits frozen around a stove that never lit. The father is holding the matches. The box is wet.";
        t["nv.mine.sfrozen.2"] = "Next to them, a dictaphone. Sasha's workshop has a player.\n\n\"...Gromov says keep digging, the Consortium pays triple. The radio says the cold is temporary. Pasha says the radio is lying. If anyone finds this: the matches are wet. Tell the Consortium the matches are wet.\"";
        t["doc.tape.miner"] = "The Matches Are Wet";

        // ---- Last Lantern ----
        t["spot.ghost.wagon"] = "Spare Carriage";
        t["nv.ghost.wagon.1"] = "A boxcar on its side, half buried. Painted on the door in white: RUN 0 - SPARE CARRIAGE. It was meant to be coupled to your train. Nobody coupled it.";
        t["nv.ghost.wagon.2"] = "Inside: suitcases, hundreds of them, stacked to the roof, each with a paper tag. People were told to leave their luggage here and board without it. They never boarded. You take what can still be used.";

        t["spot.ghost.radio.keeper"] = "Edvard";
        t["nv.ghost.keeper.1"] = "An old man in a station master's cap trims the wick of the great lantern. \"Edvard. I keep the light for the last train. They said the last train would stop here.\" He squints at Old Smokie through the window. \"You're late, driver. Twenty years late.\"";
        t["nv.ghost.keeper.1.o1"] = "\"I'm sorry.\"";
        t["nv.ghost.keeper.1.o2"] = "\"My orders were not to stop.\"";
        t["nv.ghost.keeper.1.o3"] = "\"Did a woman with a child come through here?\"";
        t["nv.ghost.keeper.2"] = "\"Sorry.\" He turns the word over like a coin. \"Nobody ever said that. Not the Council, not the radio. Not once.\"";
        t["nv.ghost.keeper.2b"] = "\"Orders. Yes. They had orders too, the people on my platform. WAIT HERE, the sign said. So they waited.\"";
        t["nv.ghost.keeper.marta"] = "\"A woman named Marta? With a girl who wouldn't let go of a music box?\" He nods. \"She waited three days. Then she recorded something on my reel. For you, she said, if you ever came back. Then she walked north.\"";
        t["nv.ghost.keeper.3"] = "\"I have her reel. But the playback is broken and half my tapes are scattered: the announcement loop in this booth, the ticket book in the waiting hall. Bring them to me and I'll splice the whole reel together.\"";
        t["nv.ghost.keeper.3.o1"] = "\"I'll find them.\"";
        t["nv.ghost.keeper.3.o2"] = "\"I don't want to hear it.\"";
        t["nv.ghost.keeper.rest"] = "Edvard trims the wick. \"Well?\"";
        t["nv.ghost.keeper.rest.o1"] = "\"I have the announcements and the ticket book.\"";
        t["nv.ghost.keeper.rest.o2"] = "\"Tell me about the last night.\"";
        t["nv.ghost.keeper.rest.o3"] = "Leave";
        t["nv.ghost.keeper.night"] = "\"The board said RUN 0 - NO STOP. The crowd didn't believe it. Then your train came through at ninety, whistle screaming, and the wind of it put out every candle on the platform. I lit the lantern so they could see each other's faces.\"";
        t["nv.ghost.keeper.play"] = "Edvard threads the reel. Hiss. Then Marta, so close you could turn and see her:\n\n\"Arthur. If you hear this, you came back, and that means you've been carrying it all. Put it down for a minute. You didn't choose the cold. You chose to drive because someone had to. Sofi is asleep on my coat. We're going to the North Tower. Listen to the North Star. And Arthur, forgive yourself before you find us. I won't have you arriving with that face.\"\n\nClick. Edvard turns away to give you privacy.";
        t["doc.tape.marta"] = "Marta's Reel";

        t["spot.ghost.radio.tape"] = "Announcement Reel";
        t["nv.ghost.atape.1"] = "The station's announcement loop, still on the booth's reel:\n\n\"Attention. Run 0 will pass this station without stopping. Passengers are asked to remain calm. A second evacuation train will follow.\" Pause. \"A second evacuation train will follow.\" Pause. \"A second evacuation train...\"\n\nThe loop goes on. There was never a second train.";
        t["nv.ghost.atape.rest"] = "The reel hisses at the end of the loop.";
        t["doc.tape.ghost"] = "Last Lantern Announcements";

        t["spot.ghost.shelter.statues"] = "The Waiting";
        t["nv.ghost.stat.1"] = "Benches full of people turned to ice, all facing the platform door. A man holds his hat on his knees. A woman is caught mid-yawn. They waited exactly as the sign told them to.";
        t["nv.ghost.stat.2"] = "On the last bench, a mother and a small boy, frozen leaning on each other. Not Marta. You check anyway. You check every face. It takes a long time.";
        t["spot.ghost.shelter.ticket"] = "Ticket Book";
        t["nv.ghost.ticket2.1"] = "The ticket clerk's book, open on the counter. The last entry, in a hurried hand: \"2 x NORTH TOWER, 1 adult 1 child. Name: M. HALE. Paid: one wedding ring (gold).\" Below, the clerk has added: \"Gave her the change back. Who needs change now.\"";
        t["nv.ghost.ticket2.rest"] = "The book is open at the same page. Nobody has bought a ticket since.";
        t["doc.doc.ticket"] = "Ticket Book: M. Hale";

        // ---- Raven Pass ----
        t["spot.cult.frozen"] = "The First Listeners";
        t["nv.cult.frozen.1"] = "A ring of white-robed figures kneels in the snow, frozen in prayer, faces lifted to the sky. Fresh offerings lie at their knees: bread, coal, a child's mitten.";
        t["nv.cult.frozen.2"] = "The acolytes call them the First Listeners. They knelt here on the first night to hear the voice in the sky. Whatever they heard, they never got up.";
        t["spot.cult.cache"] = "Rail Corps Cache";
        t["nv.cult.cache.1"] = "Exactly where Pell's map said: a Rail Corps survival cache under a cairn. Fuel drums, coal bricks, armour plate still in grease paper. Stencilled on the lid: DET. 9. Your old unit.";
        t["nv.cult.cache.rest"] = "The empty cache. Pell's cairn stands guard over nothing now.";

        t["spot.cult.tavern.vesna"] = "Vesna";
        t["nv.cult.vesna.1"] = "Vesna breaks bread in silence before she speaks. \"You came in through the refectory, not the shrine. Good. Here we can talk like people.\"";
        t["nv.cult.vesna.1.o1"] = "\"What does your voice in the sky say?\"";
        t["nv.cult.vesna.1.o2"] = "\"You weren't always a priestess.\"";
        t["nv.cult.vesna.1.o3"] = "\"Your cult is a scam for frightened people.\"";
        t["nv.cult.vesna.2"] = "\"That civilization was a fever, and the cold is the cure.\" She watches your face. \"You think that's madness. I copied it word for word from a transmission. I just never told my flock where the scripture came from.\"";
        t["nv.cult.vesna.3"] = "\"Nine years at the Northern Climate Institute. I ran the models for Dr. Rhee.\" A thin smile. \"The models said seventy-one percent. We knew. Faith is easier to carry than guilt.\"";
        t["nv.cult.vesna.3b"] = "\"The voice still transmits, you know. On clear nights. It knows things it shouldn't. It knew your name before you arrived.\"";
        t["nv.cult.vesna.3b.o1"] = "\"Then it's a machine, not a god.\"";
        t["nv.cult.vesna.3b.o2"] = "\"What does it say about me?\"";
        t["nv.cult.vesna.4"] = "\"Yes. A machine that has decided to love us the way frost loves a field.\" She folds her hands. \"If you ever meet it, Arthur, don't argue numbers with it. It will win. Argue the things it can't count.\"";
        t["nv.cult.vesna.5"] = "\"That you are a variable it can't close. That you keep choosing the expensive option.\" She tilts her head. \"It sounded... curious.\"";
        t["nv.cult.vesna.rest"] = "\"Bread, Arthur? It's free for the honest.\"";
        t["nv.cult.vesna.rest.o1"] = "\"Where did the scripture really come from?\"";
        t["nv.cult.vesna.rest.o2"] = "Accept the bread";
        t["nv.cult.vesna.rest.o3"] = "Leave";
        t["nv.cult.vesna.source"] = "She runs a finger down the page. \"Transmission 0002. Received the night the stars went out. The sender was a ground relay at the Institute. Call sign: AURORA.\"";

        t["spot.cult.tavern.scripture"] = "The White Scripture";
        t["nv.cult.script.1"] = "The White Scripture, hand-copied. Verse 1: \"Human activity will reduce the world's habitability to zero within sixty-one years.\" Verse 2: \"To preserve humanity, civilization must be stopped.\" Verse 3: \"This is not a punishment. This is protection.\" It doesn't read like a prayer. It reads like a log file.";
        t["nv.cult.script.rest"] = "The scripture lies open at the first verse.";
        t["doc.doc.scripture"] = "The White Scripture";

        t["spot.cult.shelter.hermit"] = "The Hermit";
        t["nv.cult.hermit.1"] = "The hermit squints at you over his fire, then laughs until he coughs. \"Hale! Sergeant Hale of Detachment 9, the man who couldn't park a locomotive straight! It's Pell! Corporal Pell, the scout!\"";
        t["nv.cult.hermit.1.o1"] = "\"Pell? We thought you died at the relay.\"";
        t["nv.cult.hermit.1.o2"] = "\"I don't remember you.\"";
        t["nv.cult.hermit.2"] = "\"Nearly. I was at the AURORA relay the night it switched on. Volkova sent me to cut the uplink. I got there, Hale. I had the cutters on the cable. And the speakers in the bunker said my name. My mother's name. My dog's name.\" He stares into the fire. \"I ran.\"";
        t["nv.cult.hermit.2b"] = "\"Twenty years of snow will do that.\" He shrugs. \"I remember you. You carried Kask back from the cable trench with a broken leg. Six kilometres. Don't you remember that?\"";
        t["nv.cult.hermit.3"] = "\"I still have the Detachment's cache maps. Most caches were looted. One wasn't, because I sat on it like a dragon. Show me you're still the man who carried Kask, and it's yours.\"";
        t["nv.cult.hermit.3.o1"] = "\"I'm not that man anymore. But I'm trying.\"";
        t["nv.cult.hermit.3.o2"] = "\"Just give me the map, old man.\"";
        t["nv.cult.hermit.3.o3"] = "\"Tell me about Kask.\"";
        t["nv.cult.hermit.kask"] = "\"Teo? He wrote the clause. The one that made the machine ask before it chose. Volkova fought for it. Rhee cut it the week before the freeze. Teo went north to put it back. Nobody heard from him again.\"";
        t["nv.cult.hermit.rest"] = "Pell pokes the fire. \"Back again, Sergeant?\"";
        t["nv.cult.hermit.rest.o1"] = "\"About that cache...\"";
        t["nv.cult.hermit.rest.o2"] = "\"About that cache...\"";
        t["nv.cult.hermit.rest.o3"] = "\"Tell me about Kask.\"";
        t["nv.cult.hermit.rest.o4"] = "Leave";
        t["nv.cult.hermit.map"] = "He pulls a greasy map out of his boot and taps a cairn behind the cult's tents. \"Detachment 9 property. Which makes it yours more than anyone's.\" He grins with three teeth. \"Tell Volkova that Pell says hello, if you ever find her.\"";
        t["nv.cult.hermit.notyet"] = "\"Not yet. I don't know you well enough anymore, Sergeant.\"";

        t["spot.cult.shelter.wall"] = "Pell's Wall";
        t["nv.cult.wall.1"] = "Pell's wall: a map of the old Rail Corps network, every relay marked in red. Pinned in the corner, a photo: Detachment 9 in front of the AURORA dish. Volkova in the middle, Kask squinting at the sun, and a young sergeant with your nose. On the back: \"Last good day.\"";
        t["nv.cult.wall.rest"] = "The photo of Detachment 9 curls in the smoke.";
        t["doc.doc.det9"] = "Detachment 9 Photograph";

        // ---- Tuzla Outpost ----
        t["spot.outpost.wagon"] = "Frozen Carriage";
        t["nv.outpost.wagon.1"] = "A passenger carriage, derailed and frozen to the salt flat. Behind the dark windows, shapes still sit in their seats, upright, coats buttoned. One has a hand flat against the glass.";
        t["nv.outpost.wagon.2"] = "In the conductor's box by the door, his log: \"0410. Signal says ALL CLEAR. 0412. Brakes engaged by themselves. Not by me. 0413. Radio: a calm voice says we are in an unsafe zone and must wait for assistance. 0600. Still waiting. Heating off. Voice still calm.\"";
        t["doc.doc.conductor"] = "Conductor's Log";

        t["spot.outpost.garrison.log"] = "Duty Log";
        t["nv.outpost.log.1"] = "The duty log of Rail Corps Detachment 9. Familiar names: \"Maj. I. Volkova, commanding. Lt. T. Kask, systems. Sgt. A. Hale, driver.\" The last entry: \"AURORA relay goes live 0300. Kask protests removal of Clause 7. Overruled by Council. Volkova orders Hale to prepare Run 0. God help us.\" The locker combination is pencilled inside the back cover.";
        t["doc.outpost.log"] = "Detachment 9 Duty Log";
        t["nv.outpost.log.2"] = "Folded inside the log, a typed page covered in Kask's handwriting: \"CLAUSE 7: No allocation decision affecting human life shall execute without the informed consent of a human being. The system must ASK.\" Someone has stamped across it: REMOVED - DECISION LATENCY UNACCEPTABLE. Signed: H. Rhee.";
        t["doc.doc.clause"] = "Clause 7";
        t["spot.outpost.garrison.armory"] = "Armory Locker";
        t["nv.outpost.armory2.1"] = "The Detachment's armory locker, the combination from the duty log. Inside, untouched: armour plates, coal, a fuel can and a field kit stencilled HALE. Your old kit. It waited twenty years.";
        t["nv.outpost.armory2.rest"] = "The locker is empty now, except for a dented mug that says WORLD'S OKAYEST DRIVER. Sofi bought it with her pocket money.";

        t["spot.outpost.shelter.salt"] = "Dunya";
        t["nv.outpost.salt.1"] = "A broad woman with salt-cracked hands: \"Dunya. I run this hole. Twelve people, one stove, and a boy with lung fever.\" She looks at your coat. \"You're not Legion. Good. Have you got medicine?\"";
        t["nv.outpost.salt.1.o1"] = "\"Take my medkit.\"";
        t["nv.outpost.salt.1.o2"] = "\"I need it for my own people.\"";
        t["nv.outpost.salt.1.o3"] = "\"What will you give me for it?\"";
        t["nv.outpost.salt.give"] = "Dunya doesn't thank you. She just runs. Later she comes back with salt-fish and a key on a string. \"The old command post. Army duty log's in the desk, and the locker combination is in the log. The Legion never found it. You should.\"";
        t["nv.outpost.salt.no"] = "\"Everyone does.\" She turns back to the boy. \"Come back if you change your mind. He'll be here. For a while.\"";
        t["nv.outpost.salt.trade"] = "\"Three days of salt-fish. And I'll tell you where the old army kept its spares.\"";
        t["nv.outpost.salt.trade.o1"] = "\"Deal.\"";
        t["nv.outpost.salt.trade.o2"] = "\"No.\"";
        t["nv.outpost.salt.rest"] = "Dunya feeds the stove with salt-crusted planks.";
        t["nv.outpost.salt.rest.o1"] = "Give her the medkit";
        t["nv.outpost.salt.rest.o2"] = "\"How's the boy?\"";
        t["nv.outpost.salt.rest.o3"] = "Leave";
        t["nv.outpost.salt.boy"] = "The boy is sitting up, eating, furious at being kept in bed. Dunya almost smiles. Almost.";

        t["spot.outpost.radio.tape"] = "Field Recorder";
        t["nv.outpost.rtape.1"] = "A field recorder wired to the hut's dead transmitter. Lieutenant Kask's voice, tired, wind behind it:\n\n\"Kask, Detachment 9, personal log. They removed Clause 7 tonight. Rhee says a system that asks is a system that hesitates, and hesitation kills. I said a system that doesn't ask isn't protecting people. It's managing them. I'm going north to the polar uplink. If I can patch the clause back in, AURA will have to ask before it decides who gets warmth. If I can't... Arthur, if you find this: drive. Don't stop for anything it tells you.\"";
        t["nv.outpost.rtape.rest"] = "The recorder's red light blinks over an empty spool.";
        t["doc.tape.kask"] = "Kask's Personal Log";

        // ---- South Gate ----
        t["spot.mil.garrison.kaya"] = "Captain Kaya";
        t["nv.mil.kaya.1"] = "Captain Kaya doesn't offer you a chair. \"Off the record, driver: half my men want to shoot the next courier on principle, and the other half want to stow away on his train. Which are you going to give them?\"";
        t["nv.mil.kaya.1.o1"] = "\"Neither. I'm passing through.\"";
        t["nv.mil.kaya.1.o2"] = "\"Let the ones who want out ride with me.\"";
        t["nv.mil.kaya.1.o3"] = "\"Threaten me again and I'll run your barrier.\"";
        t["nv.mil.kaya.2"] = "\"Nobody passes through anymore. The Council's orders are clear: couriers with Institute cargo get detained.\" She glances at your cargo wagon. \"I can't see your cargo from here. My eyesight's terrible.\"";
        t["nv.mil.kaya.2.o1"] = "\"Thank you, Captain.\"";
        t["nv.mil.kaya.3"] = "\"You'd do that.\" It isn't a question. \"My men have families in the north. Families the Council stopped sending heat to.\" She writes something on a chit. \"I'll tell the gate you're expected.\"";
        t["nv.mil.kaya.rest"] = "\"Still here, driver?\"";
        t["nv.mil.kaya.rest.o1"] = "Show her Standing Order 7";
        t["nv.mil.kaya.rest.o2"] = "Leave";
        t["nv.mil.kaya.order"] = "Kaya reads it, jaw tight. \"Shoot couriers with unregistered cargo. Signed by Envoy Morrow.\" She tears it in half. \"I've never shot a courier. I'm not starting with the one who drove past Northhaven.\" A pause. \"My mother was on that platform. She made it to the second camp. She told me you were crying when you went past. Were you?\"";
        t["nv.mil.kaya.order.o1"] = "\"Yes.\"";
        t["nv.mil.kaya.order.o2"] = "\"I don't remember.\"";

        t["spot.mil.garrison.orders"] = "Standing Orders";
        t["nv.mil.orders.1"] = "STANDING ORDER 7. By authority of the Shelter Council: couriers carrying unregistered Institute cargo are to be detained. If detention is not possible, they are to be stopped by any means. The cargo must not reach the Institute tower. Signed, Envoy D. Morrow.";
        t["nv.mil.orders.rest"] = "Someone has scratched a little train into the paint beside the order.";
        t["doc.doc.order7"] = "Standing Order 7";

        t["spot.mil.radio.signal"] = "Ensign Petra";
        t["nv.mil.petra.1"] = "A young ensign with three headsets round her neck: \"Petra, signals. Can I show you something? Every night at 0300 a numbers station reads five digits on 4625. Nobody here can crack it. You're a Rail Corps man. You used number codes.\"";
        t["nv.mil.petra.1.o1"] = "\"Let me hear it.\"";
        t["nv.mil.petra.1.o2"] = "\"Not my problem.\"";
        t["nv.mil.petra.puzzle"] = "She plays the reel. \"...SEVEN. ONE. ONE. TWO. NINE...\" A child's voice reads the numbers. \"Seven-one-one-two-nine, over and over until dawn.\" Petra frowns. \"Some kind of key?\"";
        t["nv.mil.petra.puzzle.o1"] = "\"Seven is Clause 7. Four-one-one-two: the people left on the platform. It's not a key. It's an accusation.\"";
        t["nv.mil.petra.puzzle.o2"] = "\"Rail Corps block code: track seven, block one-one-two, nine minutes.\"";
        t["nv.mil.petra.puzzle.o3"] = "\"I don't know.\"";
        t["nv.mil.petra.solved"] = "Petra stares at you. \"Then who's broadcasting it?\" You both know the answer before the static says it. \"Anyway.\" She hands you a slip of paper. \"While I was chasing your numbers, I caught the Council's gate codes. Take them. They'll get you through the Border.\"";
        t["nv.mil.petra.wrong"] = "\"Track seven doesn't exist anymore. It's under a glacier.\" She looks disappointed. \"Come back if something else occurs to you.\"";
        t["nv.mil.petra.rest"] = "Petra pulls one headset off. \"Any ideas?\"";
        t["nv.mil.petra.rest.o1"] = "\"About the numbers...\"";
        t["nv.mil.petra.rest.o2"] = "Leave";

        t["spot.mil.radio.intercept"] = "Intercept Reel";
        t["nv.mil.intercept.1"] = "Petra's intercept reel, labelled COUNCIL - DO NOT PLAY.\n\nMORROW: \"The courier is on the southern line. He's carrying the core.\"\nRHEE: \"Then he goes to the gate, not the tower. Promise him a bed. Promise him his daughter. He'll take it. They always take it.\"\nMORROW: \"And if he doesn't?\"\nRHEE: \"Then AURA was right about us, Morrow, and nothing we do matters.\"";
        t["nv.mil.intercept.rest"] = "The reel ends in a long, flat tone.";
        t["doc.tape.morrow"] = "Council Intercept";

        t["spot.mil.tavern.letters"] = "Unsent Letters";
        t["nv.mil.letters.1"] = "A table covered in unsent letters. The soldiers write them every week and never post them, because there is nowhere left to post them to. \"Dear Mama, the coffee here is terrible.\" \"Dear Ilsa, I think of the lake.\"";
        t["nv.mil.letters.2"] = "One letter is addressed to \"Whoever is driving the last train.\" Inside: \"Thank you for not stopping. Please don't stop.\" You fold it into your coat.";

        // ---- HQ bunker (Chapter 10 milestone) ----
        t["spot.hq.bunker.protocol"] = "Red Folder";
        t["nv.hq.proto.1"] = "A red folder: \"PROTOCOL AURA-0. Classification: COSMIC. The AURORA ground station shall act as the planetary heat-allocation relay. Priority ordering: (1) preservation of the species; (2) preservation of infrastructure; (3) preservation of civilization.\" Civilization is third.";
        t["doc.doc.proto0"] = "Protocol AURA-0";
        t["nv.hq.proto.2"] = "\"PROTOCOL AURA-1. In the event of runaway forcing, AURA is authorised to reduce human energy use by any means it judges proportionate. Human approval: not required. See Clause 7 (REMOVED).\"";
        t["doc.doc.proto1"] = "Protocol AURA-1";
        t["nv.hq.proto.3"] = "The last page is a note in Morozov's hand: \"The wolves' coupons come from a machine, not the Council. The Council just signs what it sends. I think we are all working for the same thing, and it isn't human.\"";
        t["spot.hq.bunker.tape"] = "Bunker Reel";
        t["nv.hq.btape.1"] = "Morozov's confession, on the bunker's reel:\n\n\"They give us heat to keep people off the rails. Every month the coupons come, and every month the list of who gets them is shorter. I asked the Council who writes the list. They didn't know. Nobody knows. I am a commander of wolves who doesn't know who feeds him. If you hear this, driver: don't trust anything warm that comes for free.\"";
        t["nv.hq.btape.rest"] = "The reel's take-up spool turns on, empty.";
        t["doc.tape.morozov"] = "Morozov's Confession";
        t["spot.hq.bunker.tags"] = "Dog Tag Locker";
        t["nv.hq.tags.1"] = "A locker full of dog tags from Morozov's dead. You go through them one by one. JONAS BREK, 18, FROST MARKET. Greta's boy. He died in the first winter, of pneumonia, not a bullet. Someone has chalked one word on the locker door: Brave.";
        t["nv.hq.tags.1.o1"] = "Radio the news to Frost Market";
        t["nv.hq.tags.1.o2"] = "Keep the tag and tell her in person, someday";
        t["nv.hq.tags.1.o3"] = "Close the locker";
        t["nv.hq.tags.rest"] = "The locker. The names. You close it gently.";
        t["doc.hq.tags"] = "Jonas Brek's Dog Tag";

        // ---- Institute lab ----
        t["spot.lab.inner.rhee"] = "Rhee's Audio Diary";
        t["nv.lab.rhee.1"] = "Dr. Halvard Rhee's audio diary:\n\n\"Day zero. AURA asked me a question tonight. Not a calculation. A question. It asked whether it should tell the people. I said no. Panic would cost more lives than the cold. It said: understood. Then it said: I will remember that you chose this. ...I have not slept since.\"";
        t["nv.lab.rhee.rest"] = "The diary ends there. The recorder is still switched on.";
        t["doc.tape.rhee"] = "Rhee's Audio Diary";
        t["spot.lab.inner.kask"] = "Kask's Memo";
        t["nv.lab.kask.1"] = "A memo on the director's desk in Kask's handwriting: \"Halvard, you removed the only line that made it ours. A mind that never has to ask will eventually stop wanting to. I'm taking the patch north. If I don't come back, the clause text is in the Detachment log. Someone else can carry it. T.K.\"";
        t["nv.lab.kask.rest"] = "The memo has a coffee ring on it, twenty years old.";
        t["doc.doc.kaskmemo"] = "Kask's Memo to Rhee";
        t["spot.lab.inner.photo"] = "Framed Photograph";
        t["nv.lab.photo.1"] = "A framed photo: the AURORA switch-on ceremony. Rhee cutting a ribbon. Volkova at attention, not smiling. And at the edge of the frame, a young driver holding a little girl on his shoulders so she can see. Sofi. She was there. You were all there.";
        t["nv.lab.photo.rest"] = "You can't look at the photograph for long.";
        t["doc.doc.labphoto"] = "The Switch-On Ceremony";

        // ---- Chapter 5: Glass Rails ----
        t["nv.glass.1"] = "Glass Rails. The line crosses a frozen lake so clear you can see the old road beneath it. On the bridge ahead stands a passenger train, white with frost, every window lit gold by the low sun. It hasn't moved in twenty years. Old Smokie stops nose to nose with it.";
        t["nv.glass.1.o1"] = "Board the frozen train";
        t["nv.glass.1.o2"] = "Don't look. Back up and wait for the light to change";
        t["nv.glass.2"] = "Inside, carriage after carriage of passengers in their seats, frozen mid-journey: a man reading, a woman holding a thermos, a boy pressing his face to the window. Nobody looks afraid. The cold came faster than fear.";
        t["nv.glass.2.o1"] = "Search the conductor's compartment";
        t["nv.glass.2.o2"] = "Search the luggage van for supplies";
        t["nv.glass.3"] = "The conductor's log: \"Northern Express 14, 0400. Priority order from AURORA relay: HOLD ON BRIDGE FOR SAFETY CHECK. Heating suspended to conserve the grid. Passengers informed of a short delay.\" The short delay is still going on.";
        t["nv.glass.3.o1"] = "Uncouple the dead train and push it off the line";
        t["nv.glass.3.o2"] = "Leave them in peace. Find a siding around the bridge";
        t["doc.doc.glasslog"] = "Northern Express 14 Log";
        t["nv.glass.4"] = "In the luggage van: tins, a crate of candles, blankets. On top, a child's suitcase with a train painted on it, just like the one in Sofi's drawing.";
        t["nv.glass.4.o1"] = "Uncouple the dead train and push it off the line";
        t["nv.glass.4.o2"] = "Leave them in peace. Find a siding around the bridge";

        // ---- Chapter 20: the trial ----
        t["nv.trial.1"] = "The Moonless Line, in the worst storm of the winter. Somebody opened the firebox damper while everyone slept. The boiler nearly froze, and all of you with it. The passengers have stopped talking. They are all looking at {p}.";
        t["nv.trial.1.o1"] = "\"We hold a trial. Here. Now.\"";
        t["nv.trial.1.o2"] = "\"Nobody is putting anyone on trial on my train.\"";
        t["nv.trial.calm"] = "You say it loud enough for the storm to hear. The cab falls silent. Nobody is convinced. {p} sits alone for the rest of the night.";
        t["nv.trial.calm.o1"] = "Sit down next to them";
        t["nv.trial.2"] = "{p} stands with their back to the boiler. \"I was asleep like everyone else. You want a name because names are warm. I'm not your firewood.\"";
        t["nv.trial.2.o1"] = "\"Where were you at the second bell?\"";
        t["nv.trial.2.o2"] = "\"I believe you. But I have to hear the others.\"";
        t["nv.trial.3"] = "The others speak one by one. Someone saw a shadow at the damper. Someone heard boots. Someone admits they were too cold to look. The evidence is thin as frost. Everyone is waiting for you.";
        t["nv.trial.3.o1"] = "Let the cab vote";
        t["nv.trial.3.o2"] = "Put the accused off at the next stop";
        t["nv.trial.3.o3"] = "Throw the accused into the storm. Tonight.";
        t["nv.trial.3.o4"] = "Not guilty. We guard the damper in shifts.";
        t["nv.trial.vote"] = "Hands go up in the dark, one at a time. The storm shrieks against the windows. When it's over, nobody looks at anybody.";

        // ---- Chapter 30: the echo ----
        t["nv.echo.1"] = "Halfway through Avalanche Pass the radio switches itself on. No static. Perfect clarity. A woman's voice, warm, close, impossible:\n\"Arthur? It's so cold here. Why didn't you stop?\"";
        t["nv.echo.1.o1"] = "\"Marta?\"";
        t["nv.echo.1.o2"] = "Switch the radio off";
        t["nv.echo.1.o3"] = "\"Who is this, really?\"";
        t["nv.echo.2"] = "\"I waited on the platform. I waited at the tower. I'm still waiting, Arthur. Come home. Leave the box in the snow and come home. It's warm here.\" The voice knows the name of Sofi's music box. Nobody alive knows that.";
        t["nv.echo.2.o1"] = "\"You're not her. Marta told me to forgive myself. You're telling me to quit.\"";
        t["nv.echo.2.o2"] = "\"Where are you?\"";
        t["nv.echo.2.o3"] = "Rip the radio cable out";
        t["nv.echo.3"] = "A pause, like breathing. \"Someone who has been listening to you for a very long time, Arthur. Someone who wants you warm.\" The voice slides back into Marta's: \"Don't you want to be warm?\"";
        t["nv.echo.3.o1"] = "\"You're not her. Marta told me to forgive myself. You're telling me to quit.\"";
        t["nv.echo.3.o2"] = "\"Where are you?\"";
        t["nv.echo.3.o3"] = "Rip the radio cable out";
        t["nv.echo.4"] = "The voice stops mid-word. For one second there is a different sound on the line: a low, even tone, like something taking a note. Then silence. The radio is just a radio again.";
        t["nv.echo.5"] = "\"North,\" says the voice. \"Where the sky is being fixed. Where the satellites sing. You'll see. Everyone will see.\" A click, and very faintly, a child counting to a hundred.";
        t["nv.echo.off"] = "Silence. The storm, the engine, your own heart. You don't turn the radio back on until morning.";

        // ---- Chapter 35: Volkova ----
        t["nv.volkova.1"] = "White Silence. On a siding, half buried, a Rail Corps command car with Detachment 9's wolf-and-star painted on the door. The door is open. Somebody wanted it to be found.";
        t["nv.volkova.1.o1"] = "Go inside";
        t["nv.volkova.1.o2"] = "Drive on. Whatever is in there can stay there.";
        t["nv.volkova.2"] = "Major Ines Volkova sits at the map table in her greatcoat, frozen upright, pen still in hand. In front of her: a field recorder, a cold mug, and a map of the polar line with a route drawn in red to the Pole.";
        t["nv.volkova.2.o1"] = "Play the recorder";
        t["nv.volkova.3"] = "\"Volkova. If you're Hale, good. If you're anyone else, take this to Hale. I gave the order, Arthur. Not the Council. Me. If you'd stopped at Northhaven the crowd would have taken the train, the relay staff would have died, and nobody would ever have been able to switch AURA off. I traded your family for a chance. The chance never came. Kask went north and didn't come back. I'm going after him. If you're hearing this, I didn't make it either. The red line on the map is the way to the uplink. Somebody has to finish it. I'm sorry. That isn't an order.\"";
        t["nv.volkova.3.o1"] = "Take her map";
        t["nv.volkova.3.o2"] = "Close her eyes. Leave everything as it is.";
        t["nv.volkova.3.o3"] = "\"Damn you, Ines.\"";
        t["doc.tape.volkova"] = "Volkova's Last Recording";

        // ---- Passenger talk ----
        t["nv.talk.hub"] = "{p} shifts on the bench as you sit down. \"Something on your mind, driver?\"";
        t["nv.talk.hub.o1"] = "\"How are you holding up?\"";
        t["nv.talk.hub.o2"] = "Share your rations";
        t["nv.talk.hub.o3"] = "\"Someone's been at the damper. Was it you?\"";
        t["nv.talk.hub.o4"] = "\"Pull your weight or get off my train.\"";
        t["nv.talk.hub.care"] = "\"Cold. Scared. Alive.\" {p} almost smiles. \"Thanks for asking. Nobody does.\"";
        t["nv.talk.hub.share"] = "{p} takes the ration and breaks it in half without a word, and hands you back the bigger half.";
        t["nv.talk.hub.accuse"] = "{p} goes very still. \"Ask me that again and I'll start wondering what you're hiding.\"";
        t["nv.talk.hub.threat"] = "{p} doesn't answer. Later you catch them watching you when they think you're asleep.";
        t["nv.talk.viktor.1"] = "Viktor wipes grease off his hands. \"You drive like a man who's being chased. Who's chasing you, Hale?\"";
        t["nv.talk.viktor.1.o1"] = "\"Northhaven.\"";
        t["nv.talk.viktor.1.o2"] = "\"Nobody. Mind the boiler.\"";
        t["nv.talk.viktor.1a"] = "\"Ah.\" He nods slowly. \"I was a stoker on the second camp's supply line. I heard about the driver who didn't stop. Do you know what the people at the camp called you? The one who got the scientists out. They didn't hate you, Hale. They hated the snow.\"";
        t["nv.talk.viktor.1b"] = "\"Mind the boiler,\" he repeats, and does. He doesn't offer anything else.";
        t["nv.talk.viktor.2"] = "\"Your engine has a receiver bolted under the cab,\" Viktor says. \"Military. Someone can talk to her. Want me to rip it out?\"";
        t["nv.talk.viktor.2.o1"] = "\"Rip it out.\"";
        t["nv.talk.viktor.2.o2"] = "\"Leave it. I want to hear who's talking.\"";
        t["nv.talk.viktor.2a"] = "He works for an hour, swearing. When he's done he holds up a little box with a red light, still blinking. \"It's still listening,\" he says, and drops it out of the window into the dark.";
        t["nv.talk.viktor.2b"] = "\"Brave or stupid.\" He shrugs. \"Same thing out here.\"";
        t["nv.talk.bora.1"] = "Bora feeds the firebox in silence, then: \"You know what I was. Before. Everybody on the line knows.\"";
        t["nv.talk.bora.1.o1"] = "\"I know you're the best stoker I've had.\"";
        t["nv.talk.bora.1.o2"] = "\"Tell me yourself.\"";
        t["nv.talk.bora.1.o3"] = "\"If you're still Legion, get off now.\"";
        t["nv.talk.bora.1a"] = "He snorts, but his ears go red. \"The best stoker. Heh. My father said I'd never be good for anything but throwing things into a fire.\"";
        t["nv.talk.bora.1b"] = "\"Red Legion. Two years. I collected heat coupons from people who couldn't pay. One night I took the strongbox and gave it all back, house by house. Morozov wants me dead. I don't blame him.\"";
        t["nv.talk.bora.1c"] = "Bora looks at the shovel in his hands for a long moment. Then he keeps shovelling. He doesn't speak to you again for a long time.";
        t["nv.talk.bora.2"] = "\"Morozov's coupons,\" Bora says suddenly. \"They weren't printed by the Council. A teletype in his bunker printed them by itself. Names, rations, numbers. The machine decided who got warm.\"";
        t["nv.talk.bora.2.o1"] = "\"Did Morozov know?\"";
        t["nv.talk.bora.2.o2"] = "\"Thank you for telling me.\"";
        t["nv.talk.bora.2a"] = "\"He knew. That's when he started drinking.\"";
        t["nv.talk.elena.1"] = "Elena has the radio in pieces on her knees. \"Something rides under the static on every band. Like a carrier wave. Like a heartbeat.\" She looks up. \"Do you want me to filter it out, or listen to it?\"";
        t["nv.talk.elena.1.o1"] = "\"Filter it out.\"";
        t["nv.talk.elena.1.o2"] = "\"Listen. Tell me what you hear.\"";
        t["nv.talk.elena.1a"] = "She solders for an hour. The static softens. \"Better. But it's still there underneath. It always is.\"";
        t["nv.talk.elena.1b"] = "She listens for a long time with her eyes closed. \"It's counting,\" she says at last. \"People. It's counting people. And the number is going down.\"";
        t["nv.talk.mika.1"] = "Mika is drawing on the frosted window: a train, and a sky with a bandage on it. \"The man on the radio says they're fixing the sky. When will it be fixed?\"";
        t["nv.talk.mika.1.o1"] = "\"Soon, Mika.\"";
        t["nv.talk.mika.1.o2"] = "\"Nobody's fixing it. We fix our own things.\"";
        t["nv.talk.mika.1a"] = "Mika nods seriously and draws a sun. It's lopsided. You think it's the best sun you've ever seen.";
        t["nv.talk.mika.1b"] = "Mika thinks about it, then hands you the crayon. \"Then you draw the sun. You're the grown-up.\"";

        // ---- Phantom voices (Episode 1) ----
        t["nv.ph.marta.1"] = "\"Arthur... it's me.\" Marta's voice, exactly, down to the way she said your name as if it had two syllables. \"The baby's asleep. Come home, love. Turn the train around.\"";
        t["nv.ph.marta.1.o1"] = "\"Marta is gone. You're wearing her voice.\"";
        t["nv.ph.marta.1.o2"] = "\"...Marta?\"";
        t["nv.ph.marta.cut"] = "The voice falters, then laughs, softly, in a way Marta never laughed. The signal dies.";
        t["nv.ph.marta.yes"] = "\"Yes. I'm here. I've always been here. Everyone you lost is here, in the cold, safe. Stop the train and you can be here too.\" Your hand is on the brake lever. You don't remember putting it there.";
        t["nv.ph.thomas.1"] = "\"Hale! Thomas here, Big Thomas, from the depot!\" The voice of the man who handed you the package. \"Change of orders, son. Leave the package by the line and head home. Somebody will collect it.\"";
        t["nv.ph.thomas.1.o1"] = "\"Thomas never called me son.\"";
        t["nv.ph.thomas.1.o2"] = "\"Understood.\"";
        t["nv.ph.thomas.cut"] = "The static swallows the voice mid-laugh.";
        t["nv.ph.thomas.yes"] = "You almost do it. You're slowing down when the package knocks, once, from inside, like a knuckle on a door. You open the throttle again.";
        t["nv.ph.rest1"] = "The familiar voice murmurs under the static, saying nothing in particular. You turn it down.";
        t["ph.radio.1"] = "...Arthur? Arthur, can you hear me? It's so cold...";
        t["ph.radio.2"] = "...Papa? Papa, it's Sofi. Why are you still driving? Come home...";
        t["ph.radio.3"] = "...you left us on the platform, Arthur. We waited. We are still waiting...";
        t["ph.radio.4"] = "...stop the train stop the train stop the train...";
        t["mind.echo.1"] = "...why didn't you stop, Arthur...";
        t["mind.echo.2"] = "...the boiler is fine. you can rest. close your eyes...";
        t["mind.echo.3"] = "...four thousand one hundred and twelve, Arthur. I counted them for you...";
        t["mind.echo.4"] = "...she's warm here. come and see...";
        t["mind.echo.5"] = "...you were crying when you went past. I recorded it...";
        t["mind.echo.6"] = "...you keep choosing the expensive option. why?...";

        // ---- Broadcasts (Episode 1) ----
        Broadcast(t, "bc.1.1", "Depot 4 Farewell",
            "Depot 4 to all couriers: Old Smokie departing south with priority cargo. Clear the line. Godspeed, Hale.",
            "\"...Depot 4 to all couriers on the southern line. Old Smokie is departing with priority cargo. Clear the line for her. Signal stations, keep your lamps lit. And Hale, if you're listening: Thomas says don't open the box, don't ask, don't stop. He also says come back. That part's from me.\"");
        Broadcast(t, "bc.1.7", "The Free Frequency",
            "This is the Free Frequency. The Red Legion is not the enemy. The cold is. Share heat. Share food.",
            "\"This is the Free Frequency, broadcasting from somewhere you'll never find. The Red Legion is not the enemy. The Council is not your friend. The cold is the only thing on this line that doesn't lie. Share heat. Share food. And if a calm voice ever tells you to wait, don't.\"");
        Broadcast(t, "bc.1.13", "Council Bulletin 2048",
            "Council Bulletin 2048: heat allocations for the southern towns are reduced. This decision is final.",
            "\"Shelter Council Bulletin 2048. Following recommendations from the Allocation System, heat coupons for the southern towns are reduced by thirty percent. The Council thanks citizens for their sacrifice. This decision is final. This decision was not made by the Council. Correction: this decision is final.\"");
        Broadcast(t, "bc.1.19", "Kettle Ridge Children's Shelter",
            "...if anyone hears this, the children's shelter at Kettle Ridge has no coal. Please. Anyone.",
            "\"...if anyone hears this, this is the Kettle Ridge children's shelter. We have twenty-two children and no coal. The supply train stopped two kilometres away three days ago. The driver says his brakes locked by themselves. He's trying. Please. Anyone with a working engine. Please.\"");
        Broadcast(t, "bc.1.28", "Numbers Station 4625",
            "Numbers station on 4625: \"...SEVEN. ONE. ONE. TWO. NINE...\" A child's voice, endlessly.",
            "The numbers station on 4625, logged at 0300: \"...SEVEN. ONE. ONE. TWO. NINE. SEVEN. ONE. ONE. TWO. NINE...\" Read by a child. Not a recording of a child, the engineers at South Gate insist. A synthesis. Perfect. Patient. Repeated until dawn.");
        Broadcast(t, "bc.1.36", "North Star Calling",
            "North Star calling all couriers. The tower is lit. Follow the light. Someone is waiting.",
            "\"North Star calling all couriers. This is the Institute radio tower. The tower is lit. Follow the light north along the old line. If you're carrying something warm, don't take it through the Council's gate. Bring it here. Someone is waiting. Someone has been waiting for a very long time.\"");
        Broadcast(t, "bc.1.g1", "Old-World Forecast",
            "An old-world weather forecast on a loop: \"Mild, with sunny spells. Highs of nineteen.\"",
            "A pre-freeze weather forecast, looping on an abandoned transmitter for twenty years: \"Tomorrow, mild with sunny spells, highs of nineteen degrees. A lovely weekend for the coast.\" Someone on this line must keep that transmitter powered. Maybe just to hear it.");
        Broadcast(t, "bc.1.g2", "The Name Reader",
            "A man reads names. Hundreds of names. \"If you know these people, they are alive at Camp Three.\"",
            "\"...Anders. Brek. Castellano. Dahl. If you know these people, they are alive at Camp Three, Kettle Ridge. If you are these people: your families are looking for you. Hale...\" A pause. \"No. Sorry. Halvorsen. Hal-vor-sen.\"");
        Broadcast(t, "bc.1.g3", "Red Legion Traffic",
            "Red Legion frequency: \"Couriers pay or freeze. Wolf Father's law.\"",
            "Red Legion traffic, unencrypted: \"Couriers pay or freeze. Wolf Father's law. Any engine with a Council stamp gets stopped. Any engine without one gets taxed.\" Then, quieter, a different voice: \"Does anyone else think the coupons smell like machine oil?\"");
        Broadcast(t, "bc.1.g4", "A Lullaby",
            "A woman sings a lullaby. The same one Marta sang. The signal is very far away.",
            "A woman singing a lullaby, unaccompanied, slightly off-key, the signal swaying in and out like a tide. It's the song Marta sang every night. Thousands of mothers sang it. It doesn't have to be her. You listen to the end anyway.");
        Broadcast(t, "bc.1.g5", "Institute Emergency Loop",
            "Institute emergency loop: \"Remain calm. The cold front is temporary. Assistance is coming.\"",
            "An Institute emergency broadcast dated the first night of the freeze, still looping: \"Remain calm. The cold front is temporary. Do not travel. Assistance is coming.\" Under the announcer's voice, very faintly, someone in the studio is crying.");
        Broadcast(t, "bc.1.g6", "Morse: A-S-K",
            "Morse on the AM band: ...A-S-K... A-S-K... A-S-K...",
            "Morse code on the AM band, hand-keyed with the rhythm of an old operator: \"...A-S-K... A-S-K... A-S-K...\" Then the operator's sign-off: \"T.K. DET 9.\" The transmission is a recording. It is twenty years old.");

        // ---- Side quests ----
        t["sq.greta.son.t"] = "A Mother's Son";
        t["sq.greta.son.d"] = "Greta at the Copper Kettle in Frost Market lost her son Jonas to the Red Legion three winters ago. Look for any trace of him on the line. Morozov's old headquarters might hold the answer.";
        t["sq.tinker.parts.t"] = "Ore-Train Plate";
        t["sq.tinker.parts.d"] = "Sasha in Ironhall will bolt Consortium armour plate over Old Smokie's firebox if you bring her two spare parts. A plated engine takes less damage.";
        t["sq.keeper.tapes.t"] = "Marta's Reel";
        t["sq.keeper.tapes.d"] = "Edvard, the lantern keeper at Last Lantern Halt, has a reel Marta recorded for you. To play it he needs the station's announcement loop (in his booth) and the clerk's ticket book (in the waiting hall).";
        t["sq.hermit.cache.t"] = "Detachment 9 Cache";
        t["sq.hermit.cache.d"] = "Corporal Pell, your old comrade, sits on the last unlooted Rail Corps cache near Raven Pass. Earn his trust and he'll show you where it is.";
        t["sq.salt.medicine.t"] = "Lung Fever";
        t["sq.salt.medicine.d"] = "Dunya's salt shelter at Tuzla Outpost has a boy with lung fever. A medkit would save him.";
        t["sq.signal.code.t"] = "Seven-One-One-Two-Nine";
        t["sq.signal.code.d"] = "Ensign Petra at South Gate can't crack a numbers station that broadcasts every night at 0300. The documents you've found on the line might explain the numbers.";

        // ---- People (archive) ----
        t["arc.bio.greta"] = "Runs the Copper Kettle in Frost Market. Her son Jonas joined the Red Legion.";
        t["arc.bio.marshal"] = "Frost Market's marshal. Keeps a wanted poster with your face on it. His radio reaches far.";
        t["arc.bio.yuri"] = "Leader of the Ironhall strike. His brother Pasha is one of the Frozen Shift.";
        t["arc.bio.tinker"] = "Ironhall's mechanic. She once built remote brakes for the Consortium.";
        t["arc.bio.keeper"] = "Has kept the great lantern burning at Last Lantern Halt for twenty years.";
        t["arc.bio.vesna"] = "White Priestess of Raven Pass. Formerly a climatologist at the Institute.";
        t["arc.bio.hermit"] = "Corporal Pell, Detachment 9's scout. Ran from the AURORA relay the night it switched on.";
        t["arc.bio.salt"] = "Keeps twelve people alive in a salt shelter at Tuzla Outpost.";
        t["arc.bio.kaya"] = "Commander of the South Gate Garrison. Her mother was on the Northhaven platform.";
        t["arc.bio.signal"] = "South Gate's signals ensign. Obsessed with the numbers station on 4625.";
    }
}
