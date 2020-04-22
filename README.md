
# DCS_Radio_Mission_Mod
Modification of DCS mission file to set Radio presets from XLM template

This is a fairly simple Console Application meant to set radio channel presets for all DCS aircraft, primarily used for multiplayer sorties to simplify channel dispatching and SimpleRadio (SRS) communications.

## Usage:
DCS_Radio_Mission_Mod.exe Mission_File.miz [Template_File.xml]

Template file is optional, by default it will look for 'template.xml' in the working directory.

## What does it do?
Most aircraft equipped with radios have a Lua array inside the mission file that defines all the presets for each radio.
The old A-10C is slightly different and uses 3 folder+file to set up the radio presets.

This app parses through an XML file containing the desired preset frequencies (meant to be identical or very similar for all sorties), and (re)place them in the mission .miz file.

It's meant to help mission designers, saving them from setting all presets on all 'Client' units each new mission they create.
 ### XML Template
The XML template has to follow the default template tree, as it's validated against an XSD upon launch.
For those unfamiliar with XML, it means it wouldn't accept new fields, and presets are limited.
Basically, you can order each whole node as you'd like, but refrain from changing anything other than the frequencies (inner text).

----------
# DCS_Radio_Mission_Mod
Français

C'est une application relativement simple permettant de régler les presets radio de tous les aéronefs DCS, pour accélérer le réglage des radios lors des sessions multijoueurs et faciliter l'utilisation de SimpleRadio (SRS).

## Utilisation:
DCS_Radio_Mission_Mod.exe Fichier_Mission.miz [Fichier_Modèle.xml]

Le fichier Modèle est optionnel, par défaut le programme utilisera le fichier 'template.xml' résidant dans le dossier de lancement.

## Ca sert à quoi?
La plupart des aéronefs équipés de radio possède une section Lua dans le fichier de mission détaillant tous les presets de chaque radio.
Le A-10C, ancien module, utilise à la place 3 dossier+fichier pour régler ses radios.

Cette application analyse le fichier XML qui doit contenir les fréquences désirées pour chaque preset (le but étant de voler régulièrement avec des presets identiques ou très similaires), et les (rem)place dans le fichier de mission .miz.

Cela permet d'aider les concepteurs de mission, en leur évitant de régler à la main tous les presets de toutes les unités 'Client' à chaque nouvelle mission.

 ### Modèle XML
Le Modèle XML doit suivre une arborescence identique au modèle par défaut, car il est validé par rapport à un XSD au lancement.
Pour ceux peu familier avec le XML, cela veut dire que le modèle n'accepte pas de nouveau champ, et le nombre de presets est limité.
Simplement, cela veut dire que l'ordre des entités complètes importe peu, mais que vous devez uniquement modifier la valeur des fréquences entre les balises.
