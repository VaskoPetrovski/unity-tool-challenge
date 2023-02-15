# Character creator editor tool
This tool is used to create new characters from existing models (FBX prefabs)
## How to use
### 1. Import from project
#### Instructions
1. Select the required assets
	- select FBX model in the **GameObject** field
	- select icon (**Texture2D**) in the **Sprite** filed
2. Configure the character attributes
	- name
	- price
	- material
3.	Click on **Create Prefab**
4.	Done. A new prefab variant of the FBX model has been created and a new character has been added to the store

### 2. External Import
In its current implementation, this is more of a POC(Proof of concept) because it requires manual reimport of the project in order work properly i did a quick google search and it seems like it requires quite an effort to make it work properly directly from the editor

Even though it's not fully functional I think its a good idea to include in the tool.
#### Instructions
1. Select the required assets from the explorer windows
	- select FBX model (**.FBX**) 
	- select icon (**.PNG**) 
2.	Click on **Copy Assets**
3.	Done.  Assets have been copied inside the project and pre-selected for character creation.

### 3. Forgotten Assets

#### Instructions
1. Click on the **Search Forgotten Assets** button to list all of the unused **FBX** prefabs.
2.  Select which prefab to add to the store.
	- select a prefab for new character. **OR**
	- select multiple prefabs for multiple characters. (*NOTE: They will all have the same name, price and material*)
3. Configure the character attributes
	- name
	- price
	- material
4.	Click on **Create Prefab Variants**
5.	Done.  A new prefab variant(s) of the FBX model has been created and a new character(s) have been added to the store

#### How are unused assets found?
First, all of the FBX files are found in the project, then I'm looking for the parents (which should be an FBX prefab)   of all the prefab variants of the characters in the store  and finally I'm filtering the said parents from the FBX files.
In order to make this fully functional, all of models of the character in store should be a prefab variant of an FBX prefab model.
I'm using this approach because it's the only consistent way that I'm aware of.

