﻿/*
 * OPENIZ DATA VIEWS
 */

-- VIEW FOR CURRENT VERSIONS OF ENTITIES
CREATE OR REPLACE VIEW ENT_CUR_VRSN_VW AS
	SELECT ENT_TBL.CLS_CD_ID, ENT_TBL.DTR_CD_ID, TPL_ID, ENT_VRSN_TBL.*, STS_CD.MNEMONIC AS STS_CS FROM ENT_VRSN_TBL INNER JOIN ENT_TBL USING (ENT_ID)
	INNER JOIN CD_CUR_VRSN_VW AS STS_CD on (ENT_VRSN_TBL.STS_CD_ID = STS_CD.CD_ID)
	WHERE ENT_VRSN_TBL.OBSLT_UTC IS NULL
	ORDER BY VRSN_SEQ_ID;

-- VIEW FOR CURRENT VERSION OF PERSONS
CREATE OR REPLACE VIEW PSN_CUR_VRSN_VW AS
	SELECT ENT_CUR_VRSN_VW.*, PSN_TBL.DOB, PSN_TBL.DOB_PREC FROM ENT_CUR_VRSN_VW INNER JOIN PSN_TBL USING (ENT_VRSN_ID);
	
-- VIEW FOR CURRENT VERSION OF PATIENTS
CREATE OR REPLACE VIEW PAT_CUR_VRSN_VW AS 
	SELECT PSN_CUR_VRSN_VW.*, PAT_TBL.GNDR_CD_ID, PAT_TBL.DCSD_UTC, PAT_TBL.DCSD_PREC, PAT_TBL.MB_ORD, GNDR_CD.MNEMONIC AS GNDR_CS FROM PSN_CUR_VRSN_VW INNER JOIN PAT_TBL USING (ENT_VRSN_ID)
	LEFT JOIN CD_CUR_VRSN_VW AS GNDR_CD on (PAT_TBL.GNDR_CD_ID = GNDR_CD.CD_ID);

-- VIEW FOR CURRENT VERSION OF PROVIDERS
CREATE OR REPLACE VIEW PVDR_CUR_VRSN_VW AS
	SELECT * FROM PSN_CUR_VRSN_VW INNER JOIN PVDR_TBL USING (ENT_VRSN_ID);

-- VIEW FOR CURRENT VERSION OF MATERIALS
CREATE OR REPLACE VIEW MAT_CUR_VRSN_VW AS 
	SELECT * FROM ENT_CUR_VRSN_VW INNER JOIN MAT_TBL USING (ENT_VRSN_ID);

-- VIEW FOR CURRENT VERSION OF MANUFACTURED MATERIALS
CREATE OR REPLACE VIEW MMAT_CUR_VRSN_VW AS
	SELECT * FROM MAT_CUR_VRSN_VW INNER JOIN MMAT_TBL USING (ENT_VRSN_ID);

-- VIEW FOR CURRENT VERSION OF PLACES
CREATE OR REPLACE VIEW PLC_CUR_VRSN_VW AS
	SELECT * FROM ENT_CUR_VRSN_VW INNER JOIN PLC_TBL USING (ENT_VRSN_ID);

-- VIEW FOR CURRENT VERSION OF USERS
CREATE OR REPLACE VIEW USR_CUR_VRSN_VW AS
	SELECT PSN_CUR_VRSN_VW.*, SEC_USR_TBL.USR_NAME 
		FROM PSN_CUR_VRSN_VW INNER JOIN USR_ENT_TBL USING (ENT_VRSN_ID)
		INNER JOIN SEC_USR_TBL ON (USR_ID = SEC_USR_ID);

-- VIEW FOR ENTITY NAMES
CREATE OR REPLACE VIEW ent_cur_name_vw as 
select
	ent_name_tbl.name_id,
	ent_name_tbl.ent_id, 
	coalesce(use_cd.mnemonic, 'Other') as use, 
	coalesce(typ_cd.mnemonic, 'Other') as typ, 
	array_to_string(array_agg(phon_val_tbl.val), ' ') as val
from ent_name_tbl inner join ent_name_cmp_tbl using (name_id) 
	inner join phon_val_tbl using (val_seq_id) 
	left join cd_cur_vrsn_vw as use_cd on (use_cd_id = use_cd.cd_id)
	left join cd_cur_vrsn_vw as typ_cd on (typ_cd_id = typ_cd.cd_id) 
where obslt_vrsn_seq_id is null 
group by ent_name_tbl.name_id, ent_id, use, typ;

-- VIEW FOR ADDRESSES
CREATE OR REPLACE VIEW ent_cur_addr_vw as 
select
	ent_addr_tbl.addr_id,
	ent_addr_tbl.ent_id, 
	coalesce(use_cd.mnemonic, 'Other') as use, 
	coalesce(typ_cd.mnemonic, 'Other') as typ, 
	array_to_string(array_agg(ent_addr_cmp_val_tbl.val), ' ') as val
from ent_addr_tbl inner join ent_addr_cmp_tbl using (addr_id) 
	inner join ent_addr_cmp_val_tbl using (val_seq_id) 
	left join cd_cur_vrsn_vw as use_cd on (use_cd_id = use_cd.cd_id)
	left join cd_cur_vrsn_vw as typ_cd on (typ_cd_id = typ_cd.cd_id) 
where obslt_vrsn_seq_id is null 
group by ent_addr_tbl.addr_id, ent_id, use, typ ;

-- ENTITY CURRENT ADDRESS VIEW
CREATE OR REPLACE VIEW ENT_CUR_TEL_VW AS 
	SELECT ENT_ID, TEL_VAL, MNEMONIC AS USE_CS FROM 
		ENT_TEL_TBL INNER JOIN CD_CUR_VRSN_VW AS USE_CD ON (USE_CD.CD_ID = ENT_TEL_TBL.USE_CD_ID) WHERE OBSLT_VRSN_SEQ_ID IS NULL;

-- ALTERNATE IDENTIIFERS
CREATE OR REPLACE VIEW ENT_CUR_ID_VW AS 
	SELECT ent_id_tbl.ent_id, id_val, aut_name, oid, nsid FROM 
		ENT_ID_TBL INNER JOIN ASGN_AUT_TBL USING(AUT_ID)
		WHERE ENT_ID_TBL.OBSLT_VRSN_SEQ_ID IS NULL;

-- ACT TABLE
CREATE OR REPLACE VIEW ACT_CUR_VRSN_VW AS 
	SELECT ACT_VRSN_TBL.*, MOOD.MNEMONIC AS MOOD_CS, STS.MNEMONIC AS STATUS_CS, RSN.MNEMONIC AS REASON_CS, TYPE.MNEMONIC AS TYPE_CS FROM ACT_VRSN_TBL NATURAL JOIN ACT_TBL 
	INNER JOIN CD_CUR_VRSN_VW AS MOOD ON (MOD_CD_ID = MOOD.CD_ID)
	INNER JOIN CD_CUR_VRSN_VW AS STS ON (ACT_VRSN_TBL.STS_CD_ID = STS.CD_ID)
	LEFT JOIN CD_CUR_VRSN_VW AS TYPE ON (TYP_CD_ID = TYPE.CD_ID)
	LEFT JOIN CD_CUR_VRSN_VW AS RSN ON (RSN_CD_ID = RSN.CD_ID)
	WHERE ACT_VRSN_TBL.OBSLT_UTC IS NULL;

-- SUBSTANCE ADMINISTRATION CURRENT VIEW
CREATE OR REPLACE VIEW SBADM_CUR_VRSN_VW AS 
	SELECT ACT_CUR_VRSN_VW.*, STE_CD_ID, RTE_CD_ID, DOS_UNT_CD_ID, SEQ_ID, SITE.MNEMONIC AS SITE_CS, ROUTE.MNEMONIC AS ROUTE_CS, DOSE.MNEMONIC AS DOSE_CS  
	FROM SUB_ADM_TBL NATURAL JOIN ACT_CUR_VRSN_VW
	INNER JOIN CD_CUR_VRSN_VW AS SITE ON (STE_CD_ID = SITE.CD_ID)
	INNER JOIN CD_CUR_VRSN_VW AS ROUTE ON (RTE_CD_ID = ROUTE.CD_ID)
	INNER JOIN CD_CUR_VRSN_VW AS DOSE ON (DOS_UNT_CD_ID = DOSE.CD_ID);

-- QUANTITY OBSERVATION TABLE
CREATE OR REPLACE VIEW QOBS_CUR_VRSN_VW AS 
	SELECT ACT_CUR_VRSN_VW.*, QTY, QTY_PRC, UOM_CD_ID, INT_CD_ID, VAL_TYP, UOM.MNEMONIC AS UOM, INTR.MNEMONIC AS INTR FROM QTY_OBS_TBL 
	NATURAL JOIN OBS_TBL 
	NATURAL JOIN ACT_CUR_VRSN_VW 
	LEFT JOIN CD_CUR_VRSN_VW AS UOM ON (UOM.CD_ID = UOM_CD_ID)
	LEFT JOIN CD_CUR_VRSN_VW AS INTR ON (INTR.CD_ID = INT_CD_ID);

-- PATIENT ENCOUNTER VIEW
CREATE OR REPLACE VIEW PAT_ENC_CUR_VRSN_VW AS
	SELECT ACT_CUR_VRSN_VW.*, DSCH_DSP_CD_ID, DSCHRG.MNEMONIC AS DSCH_MNEMONIC FROM PAT_ENC_TBL NATURAL JOIN ACT_CUR_VRSN_VW
	LEFT JOIN CD_CUR_VRSN_VW AS DSCHRG ON (DSCHRG.CD_ID = DSCH_DSP_CD_ID);
