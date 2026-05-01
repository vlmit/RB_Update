<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="aa30a905-82d7-49ed-9c87-0897dfe88f60" Name="TaskRecipients" Group="Custom" InstanceType="Tasks" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="aa30a905-82d7-00ed-2000-0897dfe88f60" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="5bfa9936-bb5a-4e8f-89a9-180bfd8f75f8">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="aa30a905-82d7-01ed-4000-0897dfe88f60" Name="ID" Type="Guid Not Null" ReferencedColumn="5bfa9936-bb5a-008f-3100-080bfd8f75f8" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="aa30a905-82d7-00ed-3100-0897dfe88f60" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="d7ee24e2-3ee8-47bc-b8e0-843b5f8170fb" Name="User" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d7ee24e2-3ee8-00bc-4000-043b5f8170fb" Name="UserID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="edecb09c-b86a-41f3-83ff-0682e0da7e12" Name="UserName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="2e2cf2ec-4324-4c27-93c2-06fd466a8d65" Name="Partner" Type="Reference(Typified) Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="2e2cf2ec-4324-0027-4000-06fd466a8d65" Name="PartnerID" Type="Guid Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="95de56f2-8df7-4638-9a38-99e6aa2378de" Name="PartnerName" Type="String(255) Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="aa30a905-82d7-00ed-5000-0897dfe88f60" Name="pk_TaskRecipients">
		<SchemeIndexedColumn Column="aa30a905-82d7-00ed-3100-0897dfe88f60" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="aa30a905-82d7-00ed-7000-0897dfe88f60" Name="idx_TaskRecipients_ID" IsClustered="true">
		<SchemeIndexedColumn Column="aa30a905-82d7-01ed-4000-0897dfe88f60" />
	</SchemeIndex>
</SchemeTable>