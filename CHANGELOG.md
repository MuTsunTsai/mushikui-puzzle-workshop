
Mushikui Puzzle Workshop


0.0.0		Basic searching feature. Written in JavaScript.
0.0.1		Add multiple searching interface. Used for determine all seq. up to 6 ply.
0.0.2		Add question mark variation.

0.1.0		Rewritten in C#, making the program 50x faster.
0.1.1		Minor engine improvement. Up to 1.5x faster.
0.1.2		Add multiple searching interface. Used for determine all seq. up to 8 ply.

0.2.0		Implement transposition table. Up to 8x faster.
0.2.1		Transposition table optimized. Reduce memory usage and make it 1.05x faster.

0.3.0		Improve move generation. Up to 2x faster.
0.3.1		Implement bitboard. Up to 1.05x faster.
0.3.2		Pre-generate complete rule data. Up to 1.01x faster.
0.3.3		Remove unnecessary string operation. Up to 1.3x faster
0.3.4		Rearrange code, and fix a critical bug. No speed improvement.

0.4.0		Implement bitboard completely and fix several critical bugs. Up to 1.3x faster.
0.4.1		Avoid multi-dimension array. Up to 1.2x faster.
0.4.2		Avoid using struct. Up to 1.1x faster. Used for determine all seq. with 9 ply.

0.5.0		Different move generating function for different length. Up to 2x faster.
0.5.1		Use pieceCount to predict disambiguation and fix a critical bug. Up to 1.1x faster.
0.5.2		Minor improvement. Up to 1.1x faster.

0.6.0		Implement pieceList and fix a critical bug. Up to 1.06x faster.
0.6.1		Avoid pesudolegal moves, improve length-specific generation and fix a critical bug.
			Up to 1.3x faster.
0.6.2		Fix a minor bug that would only effect convergence method.