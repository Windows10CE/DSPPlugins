# MultithreadedFactories

Has the potential to speed up your game's logical FPS by processing each planet/factory in parallel, instead of waiting for the last to finish before starting the next. The speed up you get will heavily depend on how many factories you have going at once, but it will NOT magically improve your FPS by leaps and bounds. It will instead possibly provide a slight improvement in certain circumstances.

In testing, I was able to speed up the normal time the game spends on this section of logic by up to 20%.

The top section is without my patch, the bottom is with. All numbers are in milliseconds.
```
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 14.9772416666667
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 15.07101
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 15.0727316666667
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 15.1412333333333
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 15.1514483333333
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 15.2026766666667
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 15.4572716666667
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 15.2261966666667


[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 12.0929316666667
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 11.9325666666667
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 12.0216483333333
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 11.92569
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 12.050435
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 12.0221933333333
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 11.957115
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 12.0779033333333
[Message:MultithreadedFactories] Average total factory processing time over 60 logical frames: 12.012065
```
