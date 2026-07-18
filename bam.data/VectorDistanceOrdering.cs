namespace Bam.Data
{
    /// <summary>
    /// Carries a vector similarity ordering's column, distance operator, and query-vector parameter
    /// through the parameter pipeline. Renders as
    /// <c>{column} {operator} {prefix}{column}{number}{castSuffix}</c>, e.g.
    /// <c>"Embedding" &lt;=&gt; :Embedding1::vector</c>. The provider that creates this token supplies
    /// the operator and cast suffix; the parameter builder converts the <see cref="Vector"/> value
    /// to the provider's literal form at bind time.
    /// </summary>
    public class VectorDistanceOrdering : Comparison
    {
        /// <summary>
        /// Initializes a new VectorDistanceOrdering.
        /// </summary>
        /// <param name="column">The vector column to measure distance against.</param>
        /// <param name="oper">The provider's distance operator (e.g. <c>&lt;=&gt;</c> for pgvector cosine).</param>
        /// <param name="value">The query vector.</param>
        /// <param name="distance">The distance semantics the operator represents.</param>
        /// <param name="castSuffix">The cast appended to the parameter placeholder (default <c>::vector</c>).</param>
        /// <param name="number">An optional parameter number for parameterized queries.</param>
        public VectorDistanceOrdering(string column, string oper, Vector value, VectorDistance distance, string castSuffix = "::vector", int? number = null)
            : base(column, oper, value, number)
        {
            this.Distance = distance;
            this.CastSuffix = castSuffix;
        }

        /// <summary>
        /// Gets the distance semantics this ordering's operator represents.
        /// </summary>
        public VectorDistance Distance { get; }

        /// <summary>
        /// Gets the cast appended to the parameter placeholder so text-bound vector parameters
        /// convert server-side.
        /// </summary>
        public string CastSuffix { get; }

        /// <summary>
        /// Renders the ordering expression, e.g. <c>"Embedding" &lt;=&gt; :Embedding1::vector</c>.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} {1} {2}{3}{4}{5}", ColumnNameFormatter(ColumnName), this.Operator, ParameterPrefix, ColumnName, Number, CastSuffix);
        }
    }
}
