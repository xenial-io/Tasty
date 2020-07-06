Description: Watch mode
Title: Watch mode
Order: 50
---
Cause there is nothing special about `Tasty` it's a normal dotnet console program by default.

# 1. Run with dotnet watch

Given that you can simply use the build in `dotnet watch` to execute tests in watch mode.

```cmd
dotnet watch run
```

There is some code inside `Tasty` that will detect if it's run from `watch` mode, and will clear the screen before each run, so the UX is a little bit better.

> In the future there will be an separate `CLI` that allows you to control watch mode on a more granular scope, such as selecting which tests to run, etc.

# 2. Congratulations

You wrote your very first delicious watched tests! Next you will learn how to integrate into CI/CD with [exit-codes](70-exit-codes.html).
