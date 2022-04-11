namespace GoodsKB.DAL.Entities;

[Flags]
public enum UserRoles
{
	None = 0,
	Administrator = 1,
	Superviser = 2,
	Advisor = 4,
	Expert = 8
}