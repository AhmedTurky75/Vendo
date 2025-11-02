# Visual Diagrams - Catalog Service DDD Architecture

This folder contains visual diagrams to help understand the Domain-Driven Design architecture of the Catalog Service.

## Available Diagrams

1. **aggregate-boundaries.md** - Shows Product and Category aggregate boundaries
2. **layer-dependencies.md** - Illustrates the 4-layer architecture and dependency direction
3. **domain-event-flow.md** - Demonstrates how domain events flow through the system
4. **command-query-flow.md** - Shows CQRS pattern in action

## How to View

### ASCII Diagrams
All diagrams include ASCII art that can be viewed in any text editor or markdown viewer.

### Mermaid Diagrams
The Mermaid diagrams can be viewed in:
- **GitHub** - Automatically rendered
- **VS Code** - Install "Markdown Preview Mermaid Support" extension
- **Online** - Copy code to https://mermaid.live

## Diagram Types

### 1. Aggregate Boundaries
Shows what's included in each aggregate and how they reference each other.

### 2. Layer Dependencies
Illustrates the onion architecture with:
- Domain (core)
- Application
- Infrastructure
- API/Presentation

### 3. Domain Event Flow
Demonstrates:
- When events are raised
- How events are collected
- Event dispatching via Unit of Work
- Event handlers reacting

### 4. Command/Query Flow
Shows CQRS pattern:
- Command flow (write operations)
- Query flow (read operations)
- Separation of concerns

## Learning Path

1. Start with **aggregate-boundaries.md** to understand the domain model
2. Move to **layer-dependencies.md** to see the architecture
3. Study **domain-event-flow.md** to understand event-driven design
4. Finally, **command-query-flow.md** to see CQRS in action

## Additional Resources

- See `/docs/DDD-PATTERNS-CATALOG.md` for detailed pattern explanations
- See `/docs/AGGREGATE-DESIGN-DECISIONS.md` for aggregate design reasoning
- See `/docs/examples/` for practical code examples
