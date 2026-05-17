namespace DotNetConceptLab.Posts.Post03_HttpClientFactory.Models;

// ── What we return to our API consumers ──────────────────────────────────────
public record OrderDto(
    int     Id,
    string  Product,
    string  Description,
    int     UserId,
    decimal Amount,
    string  Status
);

// ── What our API consumers send to us ────────────────────────────────────────
public record CreateOrderRequest(
    string  Product,
    string  Description,
    int     UserId,
    decimal Amount
);

// ── What JSONPlaceholder returns — used internally for mapping ────────────────
// We use https://jsonplaceholder.typicode.com as our simulated upstream API.
// In a real project this would be your actual upstream's response model.
public record JsonPlaceholderPost(
    int    UserId,
    int    Id,
    string Title,
    string Body
);

// ── What we send to JSONPlaceholder on POST ───────────────────────────────────
public record JsonPlaceholderCreateRequest(
    string Title,
    string Body,
    int    UserId
);
