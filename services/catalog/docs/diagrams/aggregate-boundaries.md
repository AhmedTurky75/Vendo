# Aggregate Boundaries Diagram

## Product Aggregate

### ASCII Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                       PRODUCT AGGREGATE                              │
│                    (Consistency Boundary)                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │  Product (Aggregate Root)                                   │    │
│  │  ────────────────────────────────────────────────────────   │    │
│  │  + Id: Guid                                                 │    │
│  │  + TenantId: Guid                                           │    │
│  │  + CategoryId: Guid  ◄─── Reference by ID (not object!)    │    │
│  │  - _name: string                                            │    │
│  │  - _description: string                                     │    │
│  │  - _status: ProductStatus                                   │    │
│  │  - _stockQuantity: int                                      │    │
│  │  - _trackInventory: bool                                    │    │
│  │  - _lowStockThreshold: int?                                 │    │
│  │  ────────────────────────────────────────────────────────   │    │
│  │  Contains Value Objects:                                    │    │
│  │  • SKU                                                       │    │
│  │  • Money (Price, CompareAtPrice)                            │    │
│  │  • ProductImages                                            │    │
│  │  • SEOMetadata                                              │    │
│  │  • Dimensions                                               │    │
│  │  • Tags (collection)                                        │    │
│  │  ────────────────────────────────────────────────────────   │    │
│  │  Business Methods:                                          │    │
│  │  + Create() - Factory                                       │    │
│  │  + ChangePrice()                                            │    │
│  │  + UpdateStock()                                            │    │
│  │  + Publish()                                                │    │
│  │  + Discontinue()                                            │    │
│  │  + AddTag()                                                 │    │
│  │  + IsOutOfStock()                                           │    │
│  │  + IsLowStock()                                             │    │
│  └────────────────────────────────────────────────────────────┘    │
│                                                                      │
│  Value Objects (Owned by Product):                                  │
│                                                                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐          │
│  │  SKU         │  │  Money       │  │  ProductImages   │          │
│  ├──────────────┤  ├──────────────┤  ├──────────────────┤          │
│  │ + Value      │  │ + Amount     │  │ + Urls: List     │          │
│  │              │  │ + Currency   │  │ + Count          │          │
│  │ + Create()   │  │              │  │ + PrimaryImage   │          │
│  │ + Generate() │  │ + Create()   │  │                  │          │
│  └──────────────┘  │ + Add()      │  │ + Create()       │          │
│                    │ + Subtract() │  │ + AddUrl()       │          │
│  ┌──────────────┐  └──────────────┘  └──────────────────┘          │
│  │SEOMetadata   │                                                   │
│  ├──────────────┤  ┌──────────────┐  ┌──────────────────┐          │
│  │+ MetaTitle   │  │  Dimensions  │  │  Tags            │          │
│  │+ MetaDesc    │  ├──────────────┤  ├──────────────────┤          │
│  │+ MetaKeywords│  │ + Length     │  │ + Collection     │          │
│  │              │  │ + Width      │  │   of strings     │          │
│  │+ Create()    │  │ + Height     │  │                  │          │
│  └──────────────┘  │ + Unit       │  └──────────────────┘          │
│                    │              │                                 │
│                    │ + Create()   │                                 │
│                    │ + Volume     │                                 │
│                    │ + ConvertTo()│                                 │
│                    └──────────────┘                                 │
│                                                                      │
│  Invariants Protected:                                              │
│  ✓ CompareAtPrice must be > Price                                   │
│  ✓ Stock cannot be negative                                         │
│  ✓ Cannot publish without images                                    │
│  ✓ SKU must be unique (validated in Application layer)              │
│  ✓ Product name 1-200 characters                                    │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

### Why These Boundaries?

**Included in Product Aggregate:**
- ✅ SKU - Product's unique identifier
- ✅ Money (Price, CompareAtPrice) - Pricing information (invariant: CompareAtPrice > Price)
- ✅ ProductImages - Visual representation (invariant: Must have images to publish)
- ✅ SEOMetadata - Search optimization data
- ✅ Dimensions - Physical measurements
- ✅ Tags - Categorization keywords

**NOT Included (Referenced by ID):**
- ❌ Category - Separate aggregate (different lifecycle, can have 1000s of products)
- ❌ Tenant - Separate bounded context

**Reasoning:**
All included elements must be **consistent together**. For example:
- When publishing a product, we check if images exist (Product + ProductImages)
- When setting compare-at price, we verify it's greater than price (Price + CompareAtPrice)
- These checks require both pieces of data in the same transaction boundary

---

## Category Aggregate

### ASCII Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                     CATEGORY AGGREGATE                               │
│                    (Consistency Boundary)                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │  Category (Aggregate Root)                                  │    │
│  │  ────────────────────────────────────────────────────────   │    │
│  │  + Id: Guid                                                 │    │
│  │  + TenantId: Guid                                           │    │
│  │  + ParentCategoryId: Guid? ◄── Reference by ID!            │    │
│  │  - _name: string                                            │    │
│  │  - _description: string?                                    │    │
│  │  - _isActive: bool                                          │    │
│  │  - _displayOrder: int                                       │    │
│  │  ────────────────────────────────────────────────────────   │    │
│  │  Contains Value Objects:                                    │    │
│  │  • Slug                                                      │    │
│  │  • SEOMetadata                                              │    │
│  │  ────────────────────────────────────────────────────────   │    │
│  │  Business Methods:                                          │    │
│  │  + Create() - Factory                                       │    │
│  │  + ChangeName()                                             │    │
│  │  + SetParent()                                              │    │
│  │  + Activate()                                               │    │
│  │  + Deactivate()                                             │    │
│  │  + CanBeDeleted()                                           │    │
│  └────────────────────────────────────────────────────────────┘    │
│                                                                      │
│  Value Objects (Owned by Category):                                 │
│                                                                      │
│  ┌──────────────────┐  ┌────────────────────────────────┐          │
│  │  Slug            │  │  SEOMetadata                   │          │
│  ├──────────────────┤  ├────────────────────────────────┤          │
│  │ + Value          │  │ + MetaTitle                    │          │
│  │                  │  │ + MetaDescription              │          │
│  │ + Generate()     │  │ + MetaKeywords                 │          │
│  │ + Create()       │  │                                │          │
│  └──────────────────┘  │ + Create()                     │          │
│                        └────────────────────────────────┘          │
│                                                                      │
│  Invariants Protected:                                              │
│  ✓ Category cannot be its own parent                                │
│  ✓ Slug must be unique (validated in Application layer)             │
│  ✓ Name must be 1-100 characters                                    │
│  ✓ Cannot delete category with children (checked via query)         │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

### Why These Boundaries?

**Included in Category Aggregate:**
- ✅ Slug - URL-friendly identifier
- ✅ SEOMetadata - Search optimization

**NOT Included (Referenced by ID):**
- ❌ Parent Category - Another instance of same aggregate type
- ❌ Child Categories - Would create infinite loading (parent loads children, children load their children...)
- ❌ Products in Category - Separate aggregates (could be 1000s)

**Reasoning:**
- Category is kept small and focused
- Hierarchical relationships via ID references prevent circular dependencies
- Each category can be loaded/saved independently

---

## Aggregate Relationships

### ASCII Diagram

```
         Tenant (Different Bounded Context)
            │
            │ (ID Reference)
            ▼
    ┌───────────────┐                 ┌───────────────┐
    │   Category    │                 │   Product     │
    │   Aggregate   │                 │   Aggregate   │
    ├───────────────┤                 ├───────────────┤
    │ Id            │◄────────────────│ CategoryId    │
    │ Name          │  (ID Reference) │ Name          │
    │ Slug          │                 │ SKU           │
    │ SEOMetadata   │                 │ Price         │
    │               │                 │ Images        │
    │               │                 │ Dimensions    │
    └───────────────┘                 └───────────────┘
            ▲
            │ (ID Reference)
            │
    ┌───────────────┐
    │Parent Category│
    │  (Same Type)  │
    └───────────────┘

Key:
─────►  ID Reference (weak coupling)
═════►  Contains (strong coupling, same aggregate)
```

### How Aggregates Communicate

1. **ID References Only**
   ```csharp
   public class Product
   {
       public Guid CategoryId { get; private set; }  // ✅ ID only
       // NOT: public Category Category { get; set; }  ❌ Object reference
   }
   ```

2. **Loading Separately**
   ```csharp
   // Load Product aggregate
   var product = await _unitOfWork.Products.GetByIdAsync(productId);

   // Load Category aggregate separately
   var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
   ```

3. **Domain Events for Coordination**
   ```csharp
   // Product raises event
   product.ChangeCategory(newCategoryId);
   // → Raises ProductCategoryChangedEvent

   // Handler updates Category (if needed, separate transaction)
   public class ProductCategoryChangedHandler
   {
       public async Task Handle(ProductCategoryChangedEvent evt)
       {
           var category = await _repo.GetByIdAsync(evt.NewCategoryId);
           category.IncrementProductCount();  // Separate aggregate, separate transaction
           await _unitOfWork.SaveChangesAsync();
       }
   }
   ```

---

## Mermaid Diagram - Aggregate Boundaries

```mermaid
graph TB
    subgraph ProductAggregate["<b>Product Aggregate</b><br/>(Consistency Boundary)"]
        Product[Product<br/>Aggregate Root]
        SKU[SKU<br/>Value Object]
        Money[Money<br/>Value Object]
        Images[ProductImages<br/>Value Object]
        SEO1[SEOMetadata<br/>Value Object]
        Dimensions[Dimensions<br/>Value Object]
        Tags[Tags<br/>Collection]

        Product -.contains.-> SKU
        Product -.contains.-> Money
        Product -.contains.-> Images
        Product -.contains.-> SEO1
        Product -.contains.-> Dimensions
        Product -.contains.-> Tags
    end

    subgraph CategoryAggregate["<b>Category Aggregate</b><br/>(Consistency Boundary)"]
        Category[Category<br/>Aggregate Root]
        Slug[Slug<br/>Value Object]
        SEO2[SEOMetadata<br/>Value Object]

        Category -.contains.-> Slug
        Category -.contains.-> SEO2
    end

    Category -- "references by ID" --> Product
    Category -- "parent (ID)" -.-> Category

    style Product fill:#e1f5ff,stroke:#0066cc,stroke-width:3px
    style Category fill:#fff4e1,stroke:#cc6600,stroke-width:3px
    style SKU fill:#f0f0f0,stroke:#666,stroke-width:1px
    style Money fill:#f0f0f0,stroke:#666,stroke-width:1px
    style Images fill:#f0f0f0,stroke:#666,stroke-width:1px
    style SEO1 fill:#f0f0f0,stroke:#666,stroke-width:1px
    style SEO2 fill:#f0f0f0,stroke:#666,stroke-width:1px
    style Dimensions fill:#f0f0f0,stroke:#666,stroke-width:1px
    style Tags fill:#f0f0f0,stroke:#666,stroke-width:1px
    style Slug fill:#f0f0f0,stroke:#666,stroke-width:1px
```

---

## Key Takeaways

### Product Aggregate
- **Size:** Medium (~400 lines of code)
- **Entities:** 1 (Product)
- **Value Objects:** 6 (SKU, Money, ProductImages, SEOMetadata, Dimensions, Tags)
- **Invariants:** 5 protected invariants
- **Transaction Scope:** All product data changes in single transaction

### Category Aggregate
- **Size:** Small (~180 lines of code)
- **Entities:** 1 (Category)
- **Value Objects:** 2 (Slug, SEOMetadata)
- **Invariants:** 4 protected invariants
- **Transaction Scope:** Category data changes only

### Design Principles Applied

1. **Keep Aggregates Small**
   - Product: ~400 LOC ✅
   - Category: ~180 LOC ✅
   - Both manageable and focused

2. **Reference by ID**
   - Product → Category: ID reference ✅
   - Category → Parent: ID reference ✅
   - No object navigation ✅

3. **Consistency Boundaries**
   - Product + Images: Same aggregate (must publish together) ✅
   - Product + Category: Different aggregates (independent lifecycle) ✅

4. **Use Domain Events**
   - ProductPriceChangedEvent
   - ProductStockChangedEvent
   - ProductPublishedEvent
   - CategoryCreatedEvent

---

## Further Reading

- `/docs/AGGREGATE-DESIGN-DECISIONS.md` - Detailed explanations of each decision
- `/docs/DDD-PATTERNS-CATALOG.md` - Section 3: Aggregates
- `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` - Anti-patterns 5, 6, 7 (Aggregate mistakes)
