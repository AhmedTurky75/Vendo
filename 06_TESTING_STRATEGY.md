# 06_TESTING_STRATEGY.md

## 1. Purpose

This document defines the unified testing strategy across backend and frontend, following **Test-Driven Development (TDD)** principles and ensuring consistent, automated quality through all phases. Every module must be covered by meaningful tests before deployment.

---

## 2. Testing Philosophy

* **TDD First**: Write tests before implementation.
* **Automation Always**: All tests run in CI/CD via GitHub Actions.
* **Isolation and Determinism**: Each test must be independent, repeatable, and fast.
* **Clean Architecture Alignment**: Tests validate use cases and domain logic, not frameworks or infrastructure details.
* **Confidence Over Coverage**: Coverage should reflect meaningful paths, not artificial metrics.

---

## 3. Backend Testing Stack (.NET)

| Layer             | Tooling                                     | Purpose                                                    |
| ----------------- | ------------------------------------------- | ---------------------------------------------------------- |
| Unit Tests        | xUnit, FluentAssertions, Moq                | Validate domain logic, application services, and use cases |
| Integration Tests | xUnit, Testcontainers, SQL Server container | Validate infrastructure (EF Core, repositories, APIs)      |
| End-to-End (E2E)  | Postman/Newman or Playwright                | Validate full workflow (store creation → checkout → order) |

### 3.1 Unit Tests

* Location: `/src/Tests/Unit/`
* Mock dependencies using **Moq**.
* Use **FluentAssertions** for expressive assertions.
* Avoid testing EF Core or IdentityServer internals.

**Example:**

```csharp
public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Should_Create_Order_When_Valid_Request()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var handler = new CreateOrderHandler(mockRepo.Object);
        var command = new CreateOrderCommand { ProductId = 1, Quantity = 2 };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
    }
}
```

### 3.2 Integration Tests

* Location: `/src/Tests/Integration/`
* Run against a disposable **SQL Server Testcontainer**.
* Validate EF Core migrations, repositories, and APIs.
* Seed minimal test data per scenario.

**Example Setup:**

```csharp
public class IntegrationTestFixture : IAsyncLifetime
{
    public HttpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var appFactory = new WebApplicationFactory<Program>();
        Client = appFactory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

### 3.3 E2E Tests

* Tool: **Postman** collections (executed by **Newman**) or **Playwright**.
* Simulate user flows: create store → add product → checkout → verify order.
* Automatically run after successful deployment to staging.

---

## 4. Frontend Testing Stack (Angular)

| Level     | Tooling                | Purpose                                        |
| --------- | ---------------------- | ---------------------------------------------- |
| Unit      | Jest                   | Validate services, pipes, and component logic  |
| Component | Jest + Testing Library | Validate rendering, bindings, DOM interactions |
| E2E       | Playwright             | Validate user journeys and cross-module flows  |

### 4.1 Unit Tests (Jest)

* Location: `/src/app/**/*.spec.ts`
* Each component, service, and pipe must have a `.spec.ts` file.
* Use spies/mocks instead of real HTTP requests.

**Example:**

```typescript
describe('ProductService', () => {
  let service: ProductService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ProductService]
    });
    service = TestBed.inject(ProductService);
    http = TestBed.inject(HttpTestingController);
  });

  it('should fetch products', () => {
    const mockProducts = [{ id: 1, name: 'Product A' }];

    service.getProducts().subscribe(data => {
      expect(data).toEqual(mockProducts);
    });

    const req = http.expectOne('/api/products');
    req.flush(mockProducts);
  });
});
```

### 4.2 Component Tests (Testing Library)

* Validate rendered output and DOM events.
* Avoid shallow testing; use rendered component behavior.

### 4.3 E2E Tests (Playwright)

* Location: `/tests/e2e/`
* Run on build → staging deploy.
* Validate authentication, store management, checkout flow.

---

## 5. Test Data Management

* Use **Builders** or **Fakers** to create predictable test data.
* Do not use production data in any test.
* All data seeded via in-memory or isolated containers.

---

## 6. Test Coverage Goals

| Layer          | Minimum Coverage | Notes                                    |
| -------------- | ---------------- | ---------------------------------------- |
| Domain         | 95%              | Core business logic must be fully tested |
| Application    | 85%              | Handlers, validators, and mappers        |
| Infrastructure | 70%              | Focus on persistence correctness         |
| Frontend       | 80%              | Business-critical UI flows               |

---

## 7. CI/CD Integration (GitHub Actions)

* Run all tests on PR creation and before merging to `develop`.
* Fail build if:

  * Unit or integration tests fail.
  * Coverage < defined thresholds.
  * Lint errors exist.
* Generate coverage reports and store as CI artifacts.

**Pipeline stages:**

```
build → test → coverage → deploy(staging)
```

---

## 8. Testing Enforcement Rules

* PR cannot merge without all test stages passing.
* New code must include relevant unit and integration tests.
* Refactors must not reduce coverage or break tests.
* E2E tests must pass before staging deployment.
* Docs and Postman collections must reflect any endpoint or flow change.

---

## 9. Example Command Summary

### Backend

```
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### Frontend

```
npm run test -- --coverage
npx playwright test
```

### CI (GitHub Actions)

```
- run: dotnet test
- run: npm run test -- --coverage
- run: npx newman run postman/collection.json
```

---

## 10. Summary

All code must be:

* Driven by TDD.
* Backed by unit, integration, and E2E tests.
* Continuously validated by CI.
* Isolated, reproducible, and self-documenting.

> **This testing strategy ensures every feature in the system is verifiable, reliable, and production-ready.**
