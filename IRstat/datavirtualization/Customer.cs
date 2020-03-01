
namespace DataVirtualization
{
    /// <summary>
    /// Demo customer object.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

#pragma warning disable IDE0052 // Supprimer les membres privés non lus
        /// <summary>
        /// Some dummy data to give the instance a bigger memory footprint.
        /// </summary>
        private readonly byte[] data = new byte[100];
#pragma warning restore IDE0052 // Supprimer les membres privés non lus
    }
}
