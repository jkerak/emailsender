SELECT DISTINCT pa.PERSON_ID FROM PERSON_account pa
inner join account a on a.account_ID = pa.account_Id
WHERE a.ACCOUNT_STATUS_ID = 8
and a.ACCOUNT_TYPE_ID = 1
and Datediff(day, pa.ACTIVE_START, Getutcdate()) = 18
and pa.PERSON_ID NOT IN(
	SELECT PERSON_ID FROM PERSON_LOGIN_HISTORY WHERE IS_MOBILE = 1
)
  