using System;
using System.Collections.Generic;

namespace Simplify.Models;

public partial class Link
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string LinkContent { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int UserId { get; set; }

    public virtual UserAccount User { get; set; } = null!;
}
