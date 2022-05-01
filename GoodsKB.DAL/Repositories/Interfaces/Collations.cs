using MongoDB.Driver;

namespace GoodsKB.DAL.Repositories;

public static class Collations
{
	public static Collation Default => _Default ?? (_Default = Collation.Simple);
	public static Collation English_CS_AS => _English_CS_AS ?? (_English_CS_AS = new Collation(locale: "en", strength: CollationStrength.Tertiary ));
	public static Collation English_CS_AI => _English_CS_AI ?? (_English_CS_AI = new Collation(locale: "en", strength: CollationStrength.Primary, caseLevel: true ));
	public static Collation English_CI_AS => _English_CI_AS ?? (_English_CI_AS = new Collation(locale: "en", strength: CollationStrength.Secondary ));
	public static Collation English_CI_AI => _English_CI_AI ?? (_English_CI_AI = new Collation(locale: "en", strength: CollationStrength.Primary ));
	public static Collation EnglishUS_CS_AS => _EnglishUS_CS_AS ?? (_EnglishUS_CS_AS = new Collation(locale: "en_US", strength: CollationStrength.Tertiary ));
	public static Collation EnglishUS_CS_AI => _EnglishUS_CS_AI ?? (_EnglishUS_CS_AI = new Collation(locale: "en_US", strength: CollationStrength.Primary, caseLevel: true ));
	public static Collation EnglishUS_CI_AS => _EnglishUS_CI_AS ?? (_EnglishUS_CI_AS = new Collation(locale: "en_US", strength: CollationStrength.Secondary ));
	public static Collation EnglishUS_CI_AI => _EnglishUS_CI_AI ?? (_EnglishUS_CI_AI = new Collation(locale: "en_US", strength: CollationStrength.Primary ));
	public static Collation Ukrainian_CS_AS => _Ukrainian_CS_AS ?? (_Ukrainian_CS_AS = new Collation(locale: "uk", strength: CollationStrength.Tertiary ));
	public static Collation Ukrainian_CS_AI => _Ukrainian_CS_AI ?? (_Ukrainian_CS_AI = new Collation(locale: "uk", strength: CollationStrength.Primary, caseLevel: true ));
	public static Collation Ukrainian_CI_AS => _Ukrainian_CI_AS ?? (_Ukrainian_CI_AS = new Collation(locale: "uk", strength: CollationStrength.Secondary ));
	public static Collation Ukrainian_CI_AI => _Ukrainian_CI_AI ?? (_Ukrainian_CI_AI = new Collation(locale: "uk", strength: CollationStrength.Primary ));

	private static Collation? _Default;
	private static Collation? _English_CS_AS;
	private static Collation? _English_CS_AI;
	private static Collation? _English_CI_AS;
	private static Collation? _English_CI_AI;
	private static Collation? _EnglishUS_CS_AS;
	private static Collation? _EnglishUS_CS_AI;
	private static Collation? _EnglishUS_CI_AS;
	private static Collation? _EnglishUS_CI_AI;
	private static Collation? _Ukrainian_CS_AS;
	private static Collation? _Ukrainian_CS_AI;
	private static Collation? _Ukrainian_CI_AS;
	private static Collation? _Ukrainian_CI_AI;
}