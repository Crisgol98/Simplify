using System;
using System.Collections.Generic;

namespace Simplify.Models;

public partial class Note
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string NoteContent { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int UserId { get; set; }

    public virtual UserAccount User { get; set; } = null!;
}
