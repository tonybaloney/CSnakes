# Advanced Usage

This section covers advanced topics and techniques for using CSnakes in complex scenarios. These topics are designed for users who need to go beyond basic Python integration and require specialized functionality.

## Topics Covered

### [Working with Large Integers](big-integers.md)
Learn how to handle Python's arbitrary-precision integers using `BigInteger` in C#. Essential for mathematical computations, cryptography, and working with large numbers that exceed standard integer limits.

### [Free-Threading Mode](free-threading.md)
Explore Python 3.13's new free-threading mode that removes the Global Interpreter Lock (GIL) limitations. Understand how to enable it, when to use it, and what performance benefits it can provide.

### [Manual Python Integration](manual-integration.md)
Deep dive into calling Python code without the source generator. Learn how to work directly with the CSnakes runtime API for maximum control and flexibility.

### [Hot Reload Support](hot-reload.md)
Take advantage of hot reload functionality to modify Python code during development without restarting your application. Perfect for rapid iteration and debugging.

### [Signal Handler Configuration](signal-handlers.md)
Understand how Python signal handlers interact with .NET applications and learn when and how to disable them for proper integration with .NET frameworks.

### [Native AOT Support](native-aot.md)
Deploy CSnakes applications with Native AOT compilation for faster startup times and self-contained executables. Learn the requirements, limitations, and best practices.

## When to Use Advanced Features

### Large Integer Handling
- Working with cryptographic operations
- Mathematical computations requiring arbitrary precision
- Financial calculations with very large numbers
- Scientific computing with big data

### Free-Threading
- CPU-intensive Python operations
- Multi-threaded data processing
- Parallel mathematical computations
- Scenarios where you need true Python parallelism

### Manual Integration
- Dynamic Python code execution
- Custom type conversion requirements
- Advanced error handling scenarios
- Building abstractions over CSnakes runtime

### Hot Reload
- Rapid development and testing
- Interactive development workflows
- Debugging complex Python logic
- Prototyping and experimentation

### Signal Handler Management
- Web applications and services
- Applications with custom shutdown logic
- Integration with .NET hosting frameworks
- Control over application lifecycle

### Native AOT
- Performance-critical applications
- Self-contained deployment requirements
- Environments without .NET runtime
- Optimized memory usage scenarios

## Best Practices

### Performance Considerations
- Profile your application to identify bottlenecks
- Use appropriate Python types for your data
- Consider memory management implications
- Test performance with realistic workloads

### Development Workflow
- Start with source-generated bindings
- Move to manual integration only when necessary
- Use hot reload for rapid iteration
- Test advanced features in isolation

### Deployment Strategy
- Plan for Python environment packaging
- Consider Native AOT for performance gains
- Test signal handling in target environments
- Document advanced configuration requirements

### Error Handling
- Implement robust error handling for all advanced features
- Log performance metrics and errors
- Plan for graceful degradation
- Monitor production behavior

## Getting Help

If you encounter issues with advanced features:

1. **Check the specific topic documentation** for detailed troubleshooting
2. **Review the [FAQ](../community/faq.md)** for common issues
3. **Search [GitHub Issues](https://github.com/tonybaloney/CSnakes/issues)** for similar problems
4. **Create a new issue** with detailed reproduction steps

## Contributing

Found an issue or have improvements for advanced features? We welcome contributions:

- **Bug reports** for advanced functionality
- **Performance improvements** and optimizations
- **Documentation enhancements** with real-world examples
- **New advanced features** that benefit the community

See our [Contributing Guide](../community/contributing.md) for more information.

---

## Quick Reference

| Feature | Use Case | Requirements |
|---------|----------|-------------|
| [Big Integers](big-integers.md) | Large number handling | `System.Numerics.BigInteger` |
| [Free-Threading](free-threading.md) | CPU parallelism | Python 3.13+ |
| [Manual Integration](manual-integration.md) | Maximum control | Direct runtime API usage |
| [Hot Reload](hot-reload.md) | Rapid development | Development environment |
| [Signal Handlers](signal-handlers.md) | .NET integration | Framework compatibility |
| [Native AOT](native-aot.md) | Performance/deployment | Source generator required |
