# BREASTWATCH

⚠️ **Research Use Only**

This software is provided exclusively as a research tool and is **not intended for clinical use, treatment planning, or clinical decision-making**.

## Overview

This project provides a Varian Eclipse™ ESAPI script for examining External Beam Breast Treatment plans.
The software has been developed by Stefano Agostinelli, U.O. Fisica Sanitaria - AOM IRCCS Ospedale Policlinico San Martino as part of research activities in medical physics.

## Why BreastWatch: From Constraints to Context

BreastWatch is a research quality review tool designed specifically for breast radiotherapy treatment plans created in Varian Eclipse™.

Plan evaluation typically requires the inspection of multiple dose-volume metrics, treatment geometry characteristics, and planning parameters. While protocol-based dose constraints provide an essential framework for quality assessment, they may not fully capture the complexity of real-world treatment planning, where several clinically acceptable solutions can coexist for the same patient.

BreastWatch addresses this challenge by automatically extracting, organizing, and presenting clinically relevant information within a unified review environment. The script combines conventional protocol-based assessment with a complementary community-based evaluation approach derived from a reference plan library of previously reviewed treatment plans. This approach acknowledges that multiple clinically acceptable solutions may exist for the same patient and that plan quality is often better understood when interpreted within the context of historical planning experience.

<div align="center">
<img src="https://agolabs.com/wp-content/uploads/2025/05/BW_1831_main_grad-1024x562.jpg" width="80%">
</div>

*BreastWatch main interface showing both the protocol-based BW Score and the community-based CB Score for the selected treatment plan.*

The BW Score evaluates plan characteristics against predefined planning objectives and protocol constraints. The CB Score provides additional context by comparing plan metrics with the distributions observed within the reference plan library, allowing users to assess how a plan relates to historical planning outcomes and accumulated clinical experience.

While protocol compliance remains an essential component of plan review, BreastWatch extends the evaluation process by providing additional context through historical benchmarking and community-derived reference values.

To support this analysis, BreastWatch includes dedicated tools for exploring the statistical behaviour and temporal evolution of individual planning metrics within the reference dataset.

<div align="center">
<img src="https://agolabs.com/wp-content/uploads/2025/05/BW1831_datainspector_chronotab.jpg" height=260>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
<img src="https://agolabs.com/wp-content/uploads/2025/03/Fig3A.png" height=260>
</div>

*Left: historical evolution and distribution of a selected planning metric within the reference plan library. Right: positioning of the selected planning metric within the distribution of previously reviewed values.*

This combination of protocol compliance, historical benchmarking, and community-derived reference values supports a more transparent, reproducible, and experience-driven approach to breast radiotherapy plan evaluation while preserving the central role of professional clinical judgement.

## Intended Use

This software is intended **only for**:
- non-commercial scientific research
- internal evaluation

It is **not a medical device** and must not be used in any clinical workflow.

## Key Restrictions

-  No clinical use 
-  No commercial use 
-  No redistribution 
-  Internal modification allowed (non-distributable) 
-  Research use allowed 

See the `LICENSE` file for full terms.

## Data Protection

Users are fully responsible for:
- compliance with GDPR and applicable data protection laws
- proper handling of patient or sensitive data

The author and copyright holder assume **no responsibility** for any data processing performed using this software.

## Third-Party Dependencies

This software relies on the following third-party components:

- OxyPlot.Wpf (MIT License)
- Varian Eclipse Scripting API (ESAPI)

See `THIRD-PARTY-NOTICES.md` for additional information.

Varian ESAPI assemblies are not distributed with this repository and must be obtained from a licensed Eclipse installation.

## Scientific Use and Citation

If you use this software in scientific work, please cite:
["Agostinelli S, Zefiro D, Garelli S, Cavagnetto F, Gusinu M, Bevegni M, Blandino G, Fozza A, Levrero F. BreastWatch: A Varian Eclipse script tool for Community-Based automatic evaluation of breast treatment plans. J Appl Clin Med Phys. 2025 Jun;26(6):e70080. doi: 10.1002/acm2.70080. Epub 2025 Apr 1. PMID: 40170438; PMCID: PMC 12148785."](https://aapm.onlinelibrary.wiley.com/doi/full/10.1002/acm2.70080)

## Documentation and Project Blog

Visit [agolabs.com](https://agolabs.com/breastwatch-plugin-home/) for additional documentation and project updates.
