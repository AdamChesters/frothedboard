# frothedboard

Why one clipboard when eleven better

![frothedboard — ten extra clipboards on Windows, reached by holding Ctrl, tapping C, then a number](docs/frothedboard-poster.png)

Hold **Ctrl**, tap **C**, tap **3** — that copy went to board 3.
Hold **Ctrl**, tap **V**, tap **3** — board 3 comes back.

Ten boards, plus your normal clipboard, untouched. Any digit, top row or numpad. `Ctrl+X` too.
Holds anything you can copy: text, images, files, formatted documents.

Touch no digit and `Ctrl+C` / `Ctrl+V` do exactly what they always did. That's the whole point —
the other numbered clipboards make you learn a second shortcut instead.

## Install

Windows. Grab the zip from [Releases][releases], unpack it anywhere, run `frothedboard.exe`. No
installer, no runtime. It sits in the tray. A folder rather than a lone exe so nothing leaks into
`%TEMP%`.

Portable: no files written, ever. Boards live in memory and are wiped when you quit, so they
don't survive a restart — on purpose. The one thing it writes anywhere is the opt-in *Start with
Windows* toggle, which sets a single registry value and removes it when you untick it.

## Tested so far

Text copies and pastes through the boards. Cutting files works too, and boards hold their own
payloads independently — 1 and 2 each moved a different set.

Still untried: images, formatted content, and elevated windows (an unelevated hook cannot see
them; run as admin if you need them).

## Build

Microsoft's .NET 8 SDK — Ubuntu's package won't do, it strips WindowsDesktop. Cross-builds from
Linux.

```bash
dotnet test tests/Frothedboard.Core.Tests
dotnet publish src/Frothedboard.App -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o dist
```

MIT.

[releases]: https://github.com/AdamChesters/frothedboard/releases

## Feedback and support

<p align="center">
  <a href="https://discord.gg/fs4WyaQPA"><img src="assets/support/social-discord.svg" width="38" height="38" alt="Discord" title="Discord"></a>
  <a href="https://www.reddit.com/r/AdamChsLab/"><img src="assets/support/social-reddit.svg" width="38" height="38" alt="Reddit" title="Reddit"></a>
  <a href="https://github.com/AdamChesters"><img src="assets/support/social-github.svg" width="38" height="38" alt="GitHub" title="GitHub"></a>
  <a href="https://www.linkedin.com/in/adamchesters/"><img src="assets/support/social-linkedin.svg" width="38" height="38" alt="LinkedIn" title="LinkedIn"></a>
  <a href="https://adamch.com"><img src="assets/support/social-website.svg" width="38" height="38" alt="AdamCh.com" title="AdamCh.com"></a>
</p>

<p align="center">Thanks for using frothedboard. I like to fix bugs and implement features rapidly. Issue? Idea? Let's work on it! Contact me anywhere you prefer.</p>

<h3 align="center">Support</h3>

<p align="center">I love making things to have fun, share fun, and make life a lil better. If you got value out of one of them, feel free to buy us a coffee. Or a house. Push comes to shove, I prefer Aston Martin. Supporters get golden crowns on the Discord / tags on the subreddit!</p>

<p align="center">
  <a href="https://github.com/sponsors/AdamChesters"><img src="assets/support/support-github.svg" width="220" height="86" alt="GitHub Sponsors" title="GitHub Sponsors"></a>
  <a href="https://buymeacoffee.com/adamch"><img src="assets/support/support-buymeacoffee.svg" width="220" height="86" alt="Buy Me a Coffee" title="Buy Me a Coffee"></a>
  <a href="https://www.paypal.com/donate/?business=KLHSZPXTSVSAU&amp;no_recurring=0&amp;item_name=I%27ve+donated+to+lots+of+small+creators+for+their+useful+little+tools%2C+now+I+create+them.+Dig+one?+I%27d+love+your+support.&amp;currency_code=AUD"><img src="assets/support/support-paypal.svg" width="220" height="86" alt="Support with PayPal" title="Support with PayPal"></a>
</p>
