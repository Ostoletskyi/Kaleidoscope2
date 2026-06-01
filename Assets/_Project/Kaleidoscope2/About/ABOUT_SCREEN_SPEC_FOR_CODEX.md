# Codex Task — Build KAELIS About Screen

## Goal

Create an in-app **About / О программе** screen using the content in:

```text
Assets/_Project/Kaleidoscope2/About/
```

The screen must show:
- project name;
- version;
- author;
- artist pseudonym;
- copyright;
- music rights statement;
- contacts;
- YouTube handle;
- short creation story;
- credits/license access.

## Required information

Project:
**KAELIS**

Author:
**Oleksii Ostoletskyi**

Music:
**Lex Nox Lab**

Artist pseudonym note:
**Lex Nox Lab is the artistic pseudonym of Oleksii Ostoletskyi.**

YouTube:
**@Lex-Nox**

Emails:
- **ostoletskyi.oleksii@gmail.com**
- **lexnox.de@gmail.com**

Copyright:
**© 2026 Oleksii Ostoletskyi. All rights reserved.**

## Suggested menu entry

Add menu button:

```text
ABOUT
```

or

```text
О ПРОГРАММЕ
```

## Screen sections

1. Title
2. Short app description
3. Author
4. Music credits
5. Copyright / license
6. Creation story
7. Contacts
8. Back button

## UI behavior

- About screen must not change visual mode.
- About screen must not reset crystal state.
- Escape should return to previous/root menu according to current navigation rules.
- Button clicks should use existing menu audio feedback.
- Do not add new hotkey behavior unless already defined by the menu navigation system.

## Architecture

Use existing menu architecture.

Do not:
- directly mutate crystal state;
- touch runtime visual systems;
- touch file browser;
- touch slideshow;
- touch hotkey routing;
- touch shader code.

Recommended ownership:

```text
Menu Button
→ Menu Navigation / Screen Controller
→ About Screen View
→ Text content loaded from static data or serialized fields
```

## Content source

Use the markdown/text files as source material.  
For runtime UI, either:
- embed the text into serialized UI fields;
- load TextAsset files from Unity Resources/Addressables if the project already uses that pattern;
- create a small AboutContent ScriptableObject if that matches project style.

## Validation

- About button opens About screen.
- About screen displays author and music credits.
- Lex Nox Lab pseudonym is clearly explained.
- Contacts are visible.
- Copyright notice is visible.
- Back/Escape returns to menu.
- No crystal/runtime controls are changed.
- No compile errors.
