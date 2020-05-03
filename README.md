# pingraph

Ping graph - show a graph of ping round-trip times to multiple targets

The graph is logarithmic. 1ms to 20s. Bars at 10ms, 100ms, 1s, 10s.

#### TODO - some features that might be useful, and fixes
- ~Ping times are worse than in command line. Like 4-6ms vs 2-3ms to a box on my home network. I guess this is C#'s fault?~
- ~The bars are a mess~
- ~Cleanup no-title experiments~
- Restore last window position
- Re-resolve dns when host unreachable
- Minor graphics issues
  - ~Graph not properly aligned to right (latest pings not returned yet...)~
  - The title bar is not completely gone
  - ~Could be more efficient with a single DrawLines with array of points~
- More graphics
  - ~Labels on vertical axis~
  - Moving label on horizontal axis, every X seconds
  - Dynamic max if breached
    - Change manually? automatically?
- ~Tooltip on hover over line, or position, to say what's the time and RTT~
  - Do something with the time on the tooltip?
- Log to a file
- Configure stuff:
  - Horizontal resolution (hardwired 200px/30s)
  - Y min/max
  - Y bars
  - Time interval (hardwired 1s)
- ~Pause/continue (cannot pause forever as RTT memory is preallocated)~
  - Ping does not continue in background
- Scroll back in time?

