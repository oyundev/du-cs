# du-cs
A faster version of Linux/Unix "du -hs" command implemented in Rust. Part of my experiment to find out fastest Rust/Go/C# code calculating total disk usage of a directory under Windows and Linux.

This is an experiment inspired by https://github.com/artempyanykh/rdu project implementing a fast directory disk usage calculator, similar to unix/linux du tool but using rust-lang. Measurements are taken with Hyperfine v1.11.0: https://github.com/sharkdp/hyperfine. All measurements are with warm disk cache.

**Linux:** Debian GNU/Linux 9 (stretch) under WSL2 (Linux 4.19.128-microsoft-standard x86_64 GNU/Linux)<br/>
**Windows 10 x64:** Microsoft Windows [Version 10.0.19042.450]

### Disk Info:
```
Sequential write [4MB] block        Avg: 819,08MB/s
Sequential read [4MB] block         Avg: 2412,23MB/s
Random write [4KB] block            Avg: 120,39MB/s
Random read [4KB] block             Avg: 323,64MB/s
Memory copy [4MB] block             Avg: 3703,64MB/s
```

### Target Directory:
```
Files:        5403
Directories:  813
Size:         1.227.079.350 bytes
```

|Contenders    | Programming Language|  Size bytes Linux |  Size Bytes Win10 | Repo|
|----------    | -------------------|  -----------------:| ---------------:|-----|
|du/du64.exe   |  C/C++             |  105.640          |     465.784       | Linux: internal /usr/bin/du, Windows: https://docs.microsoft.com/en-us/sysinternals/downloads/du|
|rdu-async-par |  Rust				|1.243.632			|	1.126.400    	| https://github.com/artempyanykh/rdu|
|diskus        |  Rust				|801.088			|	759.808		 	| https://github.com/sharkdp/diskus  |
|du-rs         |  Rust				|649.568			|	459.776		 	| https://github.com/oyundev/du-rs	 |
|du-go         |  Go				|  1.585.304		|		1.735.168	| https://github.com/oyundev/du-go   |
|du-cs         |  C#				|  5.172.232		|		4.855.296	| https://github.com/oyundev/du-cs   |

### Notes:
 
* x86-64 Platform: amd64<br> 
* Rust version: 1.52<br>
* Go version: go1.16.3<br>
* .Net 5.0 (C# code compiled to native binary using experimental .net native AOT compiler at https://github.com/dotnet/runtimelab/tree/feature/NativeAOT/samples/HelloWorld
 
### Linux Debian 9 (WSL2):
```bash
~$ hyperfine -L exe 'du -hs',./rdu-async-par,'./du-rs -p 64 -t 64','./du-cs 64',./du-go,'./diskus --threads 64' '{exe} /mnt/r/test'
Benchmark #1: du -hs /mnt/r/test

  Time (mean ± σ):     10.058 s ±  0.375 s    [User: 183.9 ms, System: 3104.2 ms]
  Range (min … max):    9.562 s … 10.570 s    10 runs

Benchmark #2: ./rdu-async-par /mnt/r/test
  Time (mean ± σ):      5.426 s ±  0.030 s    [User: 723.1 ms, System: 6757.7 ms]
  Range (min … max):    5.390 s …  5.494 s    10 runs

Benchmark #3: ./du-rs -p 64 -t 64 /mnt/r/test
  Time (mean ± σ):      3.717 s ±  0.016 s    [User: 235.6 ms, System: 5625.0 ms]
  Range (min … max):    3.687 s …  3.735 s    10 runs

Benchmark #4: ./du-cs 64 /mnt/r/test
  Time (mean ± σ):      4.566 s ±  0.040 s    [User: 682.7 ms, System: 6599.2 ms]
  Range (min … max):    4.515 s …  4.642 s    10 runs

Benchmark #5: ./du-go /mnt/r/test
  Time (mean ± σ):      3.210 s ±  0.016 s    [User: 404.6 ms, System: 4832.7 ms]
  Range (min … max):    3.192 s …  3.231 s    10 runs

Benchmark #6: ./diskus --threads 64 /mnt/r/test
  Time (mean ± σ):      4.552 s ±  0.141 s    [User: 1.723 s, System: 7.016 s]
  Range (min … max):    4.305 s …  4.732 s    10 runs

Summary
  './du-go /mnt/r/test' ran
    1.16 ± 0.01 times faster than './du-rs -p 64 -t 64 /mnt/r/test'
    1.42 ± 0.04 times faster than './diskus --threads 64 /mnt/r/test'
    1.42 ± 0.01 times faster than './du-cs 64 /mnt/r/test'
    1.69 ± 0.01 times faster than './rdu-async-par /mnt/r/test'
    3.13 ± 0.12 times faster than 'du -hs /mnt/r/test'
```    
| Command | Mean [s] | Min [s] | Max [s] | Relative |
|:---|---:|---:|---:|---:|
| `du -hs /mnt/r/test` | 10.058 ± 0.375 | 9.562 | 10.570 | 3.13 ± 0.12 |
| `./rdu-async-par /mnt/r/test` | 5.426 ± 0.030 | 5.390 | 5.494 | 1.69 ± 0.01 |
| `./du-rs -p 64 -t 64 /mnt/r/test` | 3.717 ± 0.016 | 3.687 | 3.735 | 1.16 ± 0.01 |
| `./du-cs 64 /mnt/r/test` | 4.566 ± 0.040 | 4.515 | 4.642 | 1.42 ± 0.01 |
| `./du-go /mnt/r/test` | 3.210 ± 0.016 | 3.192 | 3.231 | 1.00 |
| `./diskus --threads 64 /mnt/r/test` | 4.552 ± 0.141 | 4.305 | 4.732 | 1.42 ± 0.04 |

### Windows 10 (x64):
```bash
PS> .\hyperfine -L exe "du64.exe -nobanner",rdu-async-par.exe,du-rs.exe,"du-cs.exe -t 64",du-go.exe,"diskus.exe --threads 64" "{exe} r:\test"
Benchmark #1: du64.exe -nobanner r:\test

  Time (mean ± σ):     703.1 ms ±   5.3 ms    [User: 1.4 ms, System: 6.2 ms]
  Range (min … max):   692.4 ms … 709.2 ms    10 runs

Benchmark #2: rdu-async-par.exe r:\test
  Time (mean ± σ):     344.9 ms ±  24.3 ms    [User: 4.1 ms, System: 5.4 ms]
  Range (min … max):   325.1 ms … 411.0 ms    10 runs

Benchmark #3: du-rs.exe r:\test
  Time (mean ± σ):      72.4 ms ±   5.1 ms    [User: 3.5 ms, System: 5.8 ms]
  Range (min … max):    66.8 ms …  93.3 ms    28 runs

Benchmark #4: du-cs.exe -t 64 r:\test
  Time (mean ± σ):      86.4 ms ±  10.5 ms    [User: 1.9 ms, System: 4.6 ms]
  Range (min … max):    71.3 ms … 109.5 ms    29 runs

Benchmark #5: du-go.exe r:\test
  Time (mean ± σ):      57.9 ms ±   2.4 ms    [User: 0.6 ms, System: 2.6 ms]
  Range (min … max):    54.1 ms …  63.5 ms    42 runs

Benchmark #6: diskus.exe --threads 64 r:\test
  Time (mean ± σ):     156.8 ms ±   4.5 ms    [User: 2.5 ms, System: 4.4 ms]
  Range (min … max):   150.4 ms … 169.4 ms    16 runs

Summary
  'du-go.exe r:\test' ran
    1.25 ± 0.10 times faster than 'du-rs.exe r:\test'
    1.49 ± 0.19 times faster than 'du-cs.exe -t 64 r:\test'
    2.71 ± 0.14 times faster than 'diskus.exe --threads 64 r:\test'
    5.96 ± 0.49 times faster than 'rdu-async-par.exe r:\test'
   12.14 ± 0.51 times faster than 'du64.exe -nobanner r:\test'
```
| Command | Mean [ms] | Min [ms] | Max [ms] | Relative |
|:---|---:|---:|---:|---:|
| `du64.exe -nobanner r:\test` | 703.1 ± 5.3 | 692.4 | 709.2 | 12.14 ± 0.51 |
| `rdu-async-par.exe r:\test` | 344.9 ± 24.3 | 325.1 | 411.0 | 5.96 ± 0.49 |
| `du-rs.exe r:\test` | 72.4 ± 5.1 | 66.8 | 93.3 | 1.25 ± 0.10 |
| `du-cs.exe -t 64 r:\test` | 86.4 ± 10.5 | 71.3 | 109.5 | 1.49 ± 0.19 |
| `du-go.exe r:\test` | 57.9 ± 2.4 | 54.1 | 63.5 | 1.00 |
| `diskus.exe --threads 64 r:\test` | 156.8 ± 4.5 | 150.4 | 169.4 | 2.71 ± 0.14 |

### Who is the winner ?

* Go is fastest of all in both platforms. I tried a lot of different approaches in Rust (without unsafe hacks) to speed it up but coroutine tasks in Go-lang are realy fast and efficient.<br>
* Sadly none of my attemts using Rust tokio-main with multithreaded runtime flavor, async-std, walkdir, rayon etc. helped to speed up rust. Only using Xudong-Huang's coroutines implementation (https://github.com/Xudong-Huang/may) almost helped to match the speed of Go implementation.<br>
* Diskus when using 64 threads is very fast and beats standard solid implementation of rdu-async-par. At some point though increasing thread count begins to degrade performance.<br>
* There is an interesting comment in diskus source code about how performance is effected by context switching in multithreading and I/O Queue. 
<br>
