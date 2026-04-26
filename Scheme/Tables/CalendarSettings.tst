<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="67b1fd42-0106-4b31-a368-ea3e4d38ac5c" Name="CalendarSettings" Group="System" InstanceType="Cards" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="67b1fd42-0106-0031-2000-0a3e4d38ac5c" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="67b1fd42-0106-0131-4000-0a3e4d38ac5c" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="64f07fbf-6091-488d-a808-a9ad42b530b1" Name="CalendarStart" Type="DateTime Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="1cfd3375-b585-495d-a28b-6cfa71925a18" Name="df_CalendarSettings_CalendarStart" Value="2013-12-31T20:00:00Z" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="336e47fb-f3c4-42a5-8b63-6f43f3392fab" Name="CalendarEnd" Type="DateTime Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="d2027c8e-f2b2-40c5-8fb4-90fd65553839" Name="df_CalendarSettings_CalendarEnd" Value="2014-12-31T19:59:59Z" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="4e369ab8-5a65-48c4-b6f5-2c84f7cda260" Name="WorkDayStart" Type="DateTime Not Null">
		<Description>Начало рабочего дня</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="d23be9cf-34fb-43b3-999f-e9bed0dacffd" Name="df_CalendarSettings_WorkDayStart" Value="1753-01-01T05:00:00Z" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="6001b5c7-7a15-4d8b-8cad-8b3b9780eb7e" Name="WorkDayEnd" Type="DateTime Not Null">
		<Description>Окончание рабочего дня</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="ca741cd8-24de-4787-8dae-dec21e419737" Name="df_CalendarSettings_WorkDayEnd" Value="1753-01-01T14:00:00Z" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="d04b2ce4-dd24-4dd2-a377-5c2a481e6bf5" Name="LunchStart" Type="DateTime Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="ce6614e0-70ad-49df-b792-b62d050337de" Name="df_CalendarSettings_LunchStart" Value="1753-01-01T09:00:00Z" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="907526c8-fc0f-413e-b4dd-7bf399c7e350" Name="LunchEnd" Type="DateTime Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="5714e196-406f-486e-b71f-f37abca2349c" Name="df_CalendarSettings_LunchEnd" Value="1753-01-01T10:00:00Z" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="67b1fd42-0106-0031-5000-0a3e4d38ac5c" Name="pk_CalendarSettings" IsClustered="true">
		<SchemeIndexedColumn Column="67b1fd42-0106-0131-4000-0a3e4d38ac5c" />
	</SchemePrimaryKey>
</SchemeTable>