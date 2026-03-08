# PrayTimes Reference Implementation

Source: https://praytimes.org  
License: MIT  
Version: 3.2

This file is stored here as a **reference specification** for prayer
time calculations.

It is **not used in production code**.

Purpose:

1. Validate our .NET prayer calculator against a known algorithm.
2. Generate golden reference vectors used in acceptance tests.
3. Provide traceability for constants, formulas, and method presets.

Golden vectors are generated from this implementation and stored in:

tests/QiblaNow.Core.Tests/TestData/praytimes-vectors.json