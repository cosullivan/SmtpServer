# SMTP Server Benchmarks

Run the focused benchmark suite with:

```bash
DOTNET_ROLL_FORWARD=Major dotnet run -c Release --project src/SmtpServer.Benchmarks/SmtpServer.Benchmarks.csproj -- --filter "*"
```

Focused runs:

```bash
DOTNET_ROLL_FORWARD=Major dotnet run -c Release --project src/SmtpServer.Benchmarks/SmtpServer.Benchmarks.csproj -- --filter "*SmtpParserBenchmarks*"
DOTNET_ROLL_FORWARD=Major dotnet run -c Release --project src/SmtpServer.Benchmarks/SmtpServer.Benchmarks.csproj -- --filter "*AuthEhloBenchmarks*"
DOTNET_ROLL_FORWARD=Major dotnet run -c Release --project src/SmtpServer.Benchmarks/SmtpServer.Benchmarks.csproj -- --filter "*DataStoreBenchmarks*"
DOTNET_ROLL_FORWARD=Major dotnet run -c Release --project src/SmtpServer.Benchmarks/SmtpServer.Benchmarks.csproj -- --filter "*BdatCommandBenchmarks*" --job short
```

## Coverage

- `SmtpParserBenchmarks` covers command parsing, including EHLO, MAIL, RCPT, AUTH, HELP, VRFY, EXPN, BDAT, PROXY, and unrecognized commands.
- `AuthEhloBenchmarks` covers EHLO response generation, successful AUTH PLAIN, and invalid AUTH PLAIN parsing.
- `ThroughputBenchmarks` covers end-to-end DATA delivery through MailKit for representative `.eml` files.
- `DataStoreBenchmarks` compares buffered DATA materialization with streaming DATA draining.
- `BdatCommandBenchmarks` compares buffered BDAT materialization with streaming BDAT draining for 1 KiB and 64 KiB bodies.

## Initial Optimization Candidates

1. Parser multi-segment handling still has known gaps in `TokenReader.TryMake<TOut1,TOut2>`; benchmark any segment-aware parser changes against single-segment command lines.
2. Command-line size enforcement currently shares message-size plumbing; measure command parsing and body reads separately before splitting the limit path.
3. Review advertised SMTP extensions against parser/state-machine coverage so EHLO only advertises behavior the server actually implements.

## SS-13 BDAT Copy Reduction

Focused command:

```bash
DOTNET_ROLL_FORWARD=Major dotnet run -c Release --project src/SmtpServer.Benchmarks/SmtpServer.Benchmarks.csproj -- --filter "*BdatCommandBenchmarks.BdatLastChunk*" --job short
```

Short-run results on .NET 9.0.17:

| Mode | Body Size | Before | After |
| --- | ---: | ---: | ---: |
| BufferedMaterializing | 1 KiB | 2.610 us, 7.34 KB | 2.448 us, 6.23 KB |
| BufferedMaterializing | 64 KiB | 65.656 us, 386.48 KB | 55.955 us, 322.36 KB |
| StreamingDrain | 1 KiB | 4.656 us, 5.05 KB | 4.651 us, 5.05 KB |
| StreamingDrain | 64 KiB | 19.393 us, 9.55 KB | 20.303 us, 9.55 KB |

The buffered BDAT fallback now builds the final `ReadOnlySequence<byte>` from the existing `MemoryStream` backing buffer instead of copying the complete message with `ToArray()`. Streaming BDAT allocation stayed unchanged.
